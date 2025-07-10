using System;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class VerboseLoggingBusinessObject : CodeDescription<ZDateTime>
	{
		public VerboseLoggingBusinessObject()
		{
			CodeMaxLength = 4;
		}

		[List(nameof(Codes))]
		public override ZString Code
		{
			get => base.Code;
			set
			{
				base.Code = value;
				base.Description = Codes.GetMultilingualDescriptionFromCode(value);
			}
		}

		[ReadOnlyMember(nameof(DescriptionReadonly))]
		public override MultilingualString Description
		{
			get => base.Description;
			set => base.Description = value;
		}

		[ReadOnlyMember(nameof(DescriptionReadonly))]
		public override ZString EnglishDescription
		{
			get => base.EnglishDescription;
			set => base.EnglishDescription = value;
		}

		bool DescriptionReadonly => true;

		public CodeDescriptionPairList Codes
		{
			get
			{
				var list = codes;
				if (list != null)
				{
					return list;
				}

				codes = new CodeDescriptionPairList();
				foreach (var pair in ObjectFactory
							.Get<IHostedServiceCodeDescriptionProvider>()
							.GetHostedServices()
							.OrderBy(codeDescription => codeDescription.Code))
				{
					codes.Add(new CodeDescriptionPair(pair.Code, pair.Description));
				}

				return codes;
			}
		}

		public bool VerboseLogging => Value > ZDateTime.UtcNow;

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new VerboseLoggingBusinessObject();
		}

		protected override ZDateTime ValueFromString(string value)
		{
			return new ZDateTime(value, DateTimeKind.Utc);
		}

		protected override void ValidateCodeCore()
		{
			ClearRowNotifications();

			if (!SystemDefined && !Codes.ContainsCode(Code))
			{
				CodeInfo.AddError(Res.GetString("{44A714C9-66E7-4E5B-A5B4-D1370E225F73}", "Code does not match any service task."));
			}

			base.ValidateCodeCore();
		}

		protected override void ValidateValueCore()
		{
			ClearRowNotifications();

			if (Value >= ZDateTime.UtcNow.AddDays(30))
			{
				ValueInfo.AddError(Res.GetString("{1582D660-ED74-4A35-9A2A-8A840580E02E}", "Verbose logging period must be less than 30 days."));
			}
			else if (Value >= ZDateTime.UtcNow.AddDays(7))
			{
				ValueInfo.AddWarning(Res.GetString("{CA15C763-EDBA-4E51-B2C0-31E03A2A3B05}", "Verbose logging period is better to be less than 7 days."));
			}

			base.ValidateValueCore();
		}

		CodeDescriptionPairList codes;
	}
}
