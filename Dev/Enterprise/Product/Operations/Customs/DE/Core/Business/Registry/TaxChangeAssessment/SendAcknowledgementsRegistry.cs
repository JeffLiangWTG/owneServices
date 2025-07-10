using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Customs.DE.Business.Res;

namespace Enterprise.Customs.DE.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.DE.Business.XmlSerializers")]
	public class SendAcknowledgementsRegistry : RegistryBusinessObjectTemplate
	{
		public SendAcknowledgementsRegistry() : base()
		{
		}

		public SendAcknowledgementsRegistry(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		public static class Schema
		{
			public const string EBSCode = nameof(SendAcknowledgementsRegistry.EBSCode);
			public const string SendGroupPK = nameof(SendAcknowledgementsRegistry.SendGroupPK);
		}

		[List(nameof(EBSCodeList))]
		[ResourceStringData("6CFDD04E-34CF-4041-B233-7E6F9EC89E56", Caption = "EORI Branch Suffix")]
		public ZString EBSCode
		{
			get => fEBSCode;
			set
			{
				SetNonPersistentPropertyValue(EBSCodeInfo, ref fEBSCode, value);
				if (!IsValidationSuspended)
				{
					ValidateEBSCode();
				}
			}
		}
		ZString fEBSCode;

		public ZPropertyInfo EBSCodeInfo => GetZPropertyInfo(nameof(EBSCode));

		[List(nameof(SendGroupList))]
		[ResourceStringData("FFABEABE-76D5-4EF2-8553-838E711B0A06", Caption = "E-Mail Recipient Group")]
		public ZGuid SendGroupPK
		{
			get { return fSendGroupPK; }
			set
			{
				SetNonPersistentPropertyValue(SendGroupPKInfo, ref fSendGroupPK, value);
				if (!IsValidationSuspended)
				{
					ValidateSendGroupPK();
				}
			}
		}
		ZGuid fSendGroupPK;

		public ZPropertyInfo SendGroupPKInfo => GetZPropertyInfo(Schema.SendGroupPK);

		#region Lookups

		public CodeDescriptionPairList EBSCodeList => fEBSCodeList ?? (fEBSCodeList = GetEBSCodeList());
		CodeDescriptionPairList fEBSCodeList;

		CodeDescriptionPairList GetEBSCodeList()
		{
			var result = new CodeDescriptionPairList();
			var currentFallbackLevel = CurrentFallbackLevel;
			if (currentFallbackLevel != null)
			{
				var company = CurrentFactory.Load<GlbCompany>(currentFallbackLevel.CompanyPK(false));
				company?.ActiveBranches
					.Select(e => e.OrgProxy)
					.Append(company.OrgProxy)
					.SelectMany(GetOrgCodesForProxy)
					.Distinct()
					.ForEach(x => result.AddPair(x, x));
			}
			return result;

			IEnumerable<ZString> GetOrgCodesForProxy(OrgHeader b) => b
				?.CustomsCodes
				.GetOrgCusCodesForCodeAndCountry(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, Core.Constants.CountryCodes.Germany)
				.Select(x => x.OK_CustomsRegNo) ?? Enumerable.Empty<ZString>();
		}

		public IBusinessObjectCollection SendGroupList => fSendGroupList ?? (fSendGroupList = new GlbGroupCollection(CurrentFactory));
		IBusinessObjectCollection fSendGroupList;

		#endregion

		#region Validations

		public void ValidateEBSCode()
		{
			var targetInfo = EBSCodeInfo;
			targetInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(targetInfo);
			if (CurrentFallbackLevel != null)
			{
				ListValidation.ErrorIfInvalidCode(targetInfo);

				if (!targetInfo.HasNotifications())
				{
					var parentCollection = GetParentCollection(this, typeof(SendAcknowledgementsRegistryCollection));
					if (parentCollection != null && parentCollection.Cast<SendAcknowledgementsRegistry>().Any(x => x != this && x.EBSCode == EBSCode))
					{
						targetInfo.AddError(Res.GetString("B74AA3F3-D3E6-47CC-AEC9-37D9B6385696", "A row with this Code already exists."));
					}
				}
			}
		}

		public void ValidateSendGroupPK()
		{
			var targetInfo = SendGroupPKInfo;
			targetInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(targetInfo);
			ListValidation.ErrorIfInvalidPK(targetInfo);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateEBSCode();
			ValidateSendGroupPK();
		}

		#endregion

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			EBSCode = reader.ReadElementString(Schema.EBSCode);
			SendGroupPK = new ZGuid(reader.ReadElementString(Schema.SendGroupPK));
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.EBSCode, EBSCode);
			writer.WriteElementString(Schema.SendGroupPK, SendGroupPK.ToString());
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new SendAcknowledgementsRegistry(fallbackLevel, factory);
	}
}

