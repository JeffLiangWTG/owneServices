using System;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	#region Organization Types

	[Flags]
	public enum RatingDebtorOrgTypes
	{
		None,
		LC,
		AG,
		CNE,
		CNR,
		LCBK,
		SAG,
		RAG,
		CCUS
	}

	[Flags]
	public enum RatingJobTypes
	{
		ALL,
		CUS,
	}

	#endregion

	[DebuggerDisplay("{OrganizationType} {UseCompanyTariff} {JobType}")]
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class RatesPriorities : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string OrganizationType = "OrganizationType";
			public const string UseCompanyTariff = "UseCompanyTariff";
			public const string JobType = "JobType";
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new RatesPriorities();
		}

		#endregion

		#region Properties

		#region OrganizationType

		[List("OrganizationTypeList")]
		[MaxLength(5)]
		public ZString OrganizationType
		{
			get { return organizationType; }
			set
			{
				CheckMaximumLength(OrganizationTypeInfo, value);
				organizationType = value;
				OrganizationTypeInfo.RefreshBinding();
				ValidateOrganizationTypes();
			}
		}

		ZString organizationType;

		public RatingDebtorOrgTypes RatingOrganizationType
		{
			get
			{
				return (RatingDebtorOrgTypes)Enum.Parse(typeof(RatingDebtorOrgTypes), OrganizationType);
			}
		}

		public ZPropertyInfo OrganizationTypeInfo
		{
			get { return GetZPropertyInfo(Schema.OrganizationType); }
		}

		public void ValidateOrganizationTypes()
		{
			if (!IsValidationSuspended)
			{
				OrganizationTypeInfo.ClearAllNotifications();

				MandatoryValidation.CheckEntered(OrganizationTypeInfo);
				ListValidation.ErrorIfInvalidCode(OrganizationTypeInfo, OrganizationTypeList);
				CheckForUniqueness((RatesPriorities org) => org.OrganizationTypeInfo);
			}
		}

		void CheckForUniqueness(Func<RatesPriorities, ZPropertyInfo> getPropertyToValidate)
		{
			foreach (var collection in ParentCollections.OfType<RatesPrioritiesCollection>())
			{
				var duplicates = collection.Cast<RatesPriorities>().Where(x =>
					x != this
					&& x.OrganizationType == OrganizationType
					&& x.JobType == JobType);

				foreach (var duplicatePriority in duplicates)
				{
					var message = Res.GetString("ed076df9-223e-4866-a578-58b16e0b9671", "Please enter unique values only.");
					getPropertyToValidate(this).AddError(message);
					getPropertyToValidate(duplicatePriority).AddError(message);
				}
			}
		}

		#endregion

		#region JobType

		[List("JobTypeList")]
		[MaxLength(3)]
		public ZString JobType
		{
			get { return jobType; }
			set
			{
				CheckMaximumLength(JobTypeInfo, value);
				jobType = value;
				JobTypeInfo.RefreshBinding();
				ValidateJobTypes();
			}
		}

		ZString jobType = nameof(RatingJobTypes.ALL);

		public RatingJobTypes RatingJobType => (RatingJobTypes)Enum.Parse(typeof(RatingJobTypes), JobType);

		public ZPropertyInfo JobTypeInfo => GetZPropertyInfo(Schema.JobType);

		public void ValidateJobTypes()
		{
			if (!IsValidationSuspended)
			{
				JobTypeInfo.ClearAllNotifications();

				MandatoryValidation.CheckEntered(JobTypeInfo);
				ListValidation.ErrorIfInvalidCode(JobTypeInfo, JobTypeList);
				CheckForUniqueness((RatesPriorities org) => org.JobTypeInfo);
			}
		}

		#endregion

		#region UseCompanyTariff

		public ZBool UseCompanyTariff
		{
			get { return useCompanyTariff; }
			set
			{
				if (useCompanyTariff != value)
				{
					useCompanyTariff = value;
					UseCompanyTariffInfo.RefreshBinding();
				}
			}
		}

		ZBool useCompanyTariff = ZBool.True;

		public ZPropertyInfo UseCompanyTariffInfo
		{
			get { return GetZPropertyInfo(Schema.UseCompanyTariff); }
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList OrganizationTypeList
		{
			get
			{
				return CurrentFactory.GetCachedValue("RatesPriorities.OrganizationTypeList", () => new CodeDescriptionPairList
					{
						new CodeDescriptionPair(nameof(RatingDebtorOrgTypes.LC), ResString.GetMultilingualString("58eac58f-adde-4d7f-ac61-433ee18b7dcd", "Local Client")),
						new CodeDescriptionPair(nameof(RatingDebtorOrgTypes.AG), ResString.GetMultilingualString("8cb4b444-d95d-4623-8508-6582dd2dae3b", "Agent")),
						new CodeDescriptionPair(nameof(RatingDebtorOrgTypes.CNE), ResString.GetMultilingualString("6c3842d9-6e0f-4e91-8a77-2d09b8a274fc", "Consignee")),
						new CodeDescriptionPair(nameof(RatingDebtorOrgTypes.CNR), ResString.GetMultilingualString("151dc2b8-1bd2-402f-9b00-647bde623d5d", "Consignor")),
						new CodeDescriptionPair(nameof(RatingDebtorOrgTypes.LCBK), ResString.GetMultilingualString("036c8777-8943-4c65-8003-6f1cc94372f0", "Local Client When Broker")),
						new CodeDescriptionPair(nameof(RatingDebtorOrgTypes.SAG), ResString.GetMultilingualString("4f04ca6a-b8e8-437e-bcc7-57d319ba7a87", "Sending Agent")),
						new CodeDescriptionPair(nameof(RatingDebtorOrgTypes.RAG), ResString.GetMultilingualString("457ad8fe-764a-4dd4-9f2b-c9920ea6c64d", "Receiving Agent")),
						new CodeDescriptionPair(nameof(RatingDebtorOrgTypes.CCUS), ResString.GetMultilingualString("483de47a-c5a6-45ba-a54d-0d14b1386d2b", "Controlling Customer"))
					});
			}
		}

		public CodeDescriptionPairList JobTypeList =>
			CurrentFactory.GetCachedValue("RatesPriorities.JobTypeList", () => new CodeDescriptionPairList
			{
				new CodeDescriptionPair(nameof(RatingJobTypes.ALL), ResString.GetMultilingualString("9E497945-B884-4EC8-9A69-AA57D2D107EF", "All")),
				new CodeDescriptionPair(nameof(RatingJobTypes.CUS), ResString.GetMultilingualString("82F17607-7A2C-4CF9-85D9-3F9C68C94BE6", "Customs"))
			});

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateOrganizationTypes();
			ValidateJobTypes();
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.OrganizationType, OrganizationType);
			writer.WriteElementString(Schema.UseCompanyTariff, UseCompanyTariff.ToString());
			writer.WriteElementString(Schema.JobType, JobType);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			OrganizationType = reader.ReadElementString(Schema.OrganizationType);

			XmlReader readerObject = reader.Reader;
			while (readerObject.NodeType != XmlNodeType.EndElement)
			{
				switch (readerObject.LocalName)
				{
					case Schema.OrganizationType:
						OrganizationType = readerObject.ReadElementString();
						break;

					case Schema.UseCompanyTariff:
						UseCompanyTariff = new ZBool(readerObject.ReadElementString());
						break;

					case Schema.JobType:
						JobType = readerObject.ReadElementString();
						break;

					default:
						readerObject.ReadElementString();
						break;
				}
			}
		}

		#endregion
	}
}
