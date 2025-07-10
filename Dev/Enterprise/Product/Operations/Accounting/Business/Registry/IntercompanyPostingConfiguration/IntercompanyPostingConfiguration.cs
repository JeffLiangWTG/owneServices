using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using AuthorisationRequirementCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class IntercompanyPostingConfiguration : RegistryBusinessObjectTemplate
	{
		#region Schema

		abstract class Schema
		{
			public const string Company = "Company";
			public const string MaxCostVarianceApprovalLevel = "MaxCostVarianceApprovalLevel";
		}

		#endregion

		public IntercompanyPostingConfiguration()
		{
		}

		public IntercompanyPostingConfiguration(ZString companyCode)
		{
			Company = companyCode;
			MaxCostVarianceApprovalLevel = AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new IntercompanyPostingConfiguration();
		}

		#region Bound Properties

		#region Company

		[MaxLength(3)]
		[ReadOnly(true)]
		public ZString Company
		{
			get { return company; }
			set
			{
				CheckMaximumLength(CompanyInfo, value);
				SetNonPersistentPropertyValue(CompanyInfo, ref company, value);
			}
		}

		public ZPropertyInfo CompanyInfo
		{
			get { return GetZPropertyInfo(Schema.Company); }
		}

		ZString company;

		#endregion

		#region MaxCostVarianceApprovalLevel

		[MaxLength(20)]
		[List("MaxCostVarianceApprovalLevelList")]
		public ZString MaxCostVarianceApprovalLevel
		{
			get { return fMaxCostVarianceApprovalLevel; }
			set
			{
				SetNonPersistentPropertyValue(MaxCostVarianceApprovalLevelInfo, ref fMaxCostVarianceApprovalLevel, value);
			}
		}

		public ZPropertyInfo MaxCostVarianceApprovalLevelInfo
		{
			get { return GetZPropertyInfo(Schema.MaxCostVarianceApprovalLevel); }
		}

		ZString fMaxCostVarianceApprovalLevel;

		#endregion

		#endregion

		#region Lookups

		#region MaxCostVarianceApprovalLevel List

		public CodeDescriptionPairList MaxCostVarianceApprovalLevelList
		{
			get
			{
				if (fMaxCostVarianceApprovalLevelList == null)
				{
					fMaxCostVarianceApprovalLevelList = new CodeDescriptionPairList();
					fMaxCostVarianceApprovalLevelList.AddPair(AuthorisationRequirementCodes.NoApprovalRequired);
					fMaxCostVarianceApprovalLevelList.AddPair(AuthorisationRequirementCodes.FirstApprovalRequiredOnly);
					fMaxCostVarianceApprovalLevelList.AddPair(AuthorisationRequirementCodes.SecondApprovalRequiredOnly);
				}
				return fMaxCostVarianceApprovalLevelList;
			}
		}
		CodeDescriptionPairList fMaxCostVarianceApprovalLevelList;

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Company, Company);
			writer.WriteElementString(Schema.MaxCostVarianceApprovalLevel, MaxCostVarianceApprovalLevel);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Company = reader.ReadElementString(Schema.Company);
			MaxCostVarianceApprovalLevel = reader.ReadElementString(Schema.MaxCostVarianceApprovalLevel);
		}

		#endregion
	}
}