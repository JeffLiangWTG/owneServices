using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ClientSharedComponents.Registry
{
	[XmlSerializerAssembly("Enterprise.ClientSharedComponents.XmlSerializers")]
	public class BranchDepartmentCodeMappingRegistryBusinessObject : RegistryBusinessObjectTemplate
	{
		public BranchDepartmentCodeMappingRegistryBusinessObject()
			: base()
		{
		}

		public BranchDepartmentCodeMappingRegistryBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Schema
		public static class Schema
		{
			public const string BranchCodePK = "BranchCodePK";
			public const string DepartmentCodePK = "DepartmentCodePK";
			public const string ProfitCentre = "ProfitCentre";
			public const string NominalDepartment = "NominalDepartment";
		}
		#endregion

		#region BranchCodePK
		public ZGuid BranchCodePK
		{
			get { return branchCodePK; }
			set
			{
				SetNonPersistentPropertyValue(BranchCodePKInfo, ref branchCodePK, value);
				if (!IsValidationSuspended)
				{
					ValidateBranchCodePK();
				}
				BranchCodePKInfo.RefreshBinding();
			}
		}
		ZGuid branchCodePK;

		public ZPropertyInfo BranchCodePKInfo
		{
			get { return GetZPropertyInfo(Schema.BranchCodePK, "BranchCode"); }
		}

		public void ValidateBranchCodePK()
		{
			BranchCodePKInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(BranchCodePKInfo);
			if (!BranchCodePKInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidPK(BranchCodePKInfo, BranchCodes);
			}
			if (!BranchCodePKInfo.HasErrors())
			{
				UniquePropertiesValidation.CheckPropertiesAreUniqueInCollection(BranchCodePKInfo, DepartmentCodePKInfo);
			}
		}
		#endregion

		#region DepartmentCodePK
		public ZGuid DepartmentCodePK
		{
			get { return departmentCodePK; }
			set
			{
				SetNonPersistentPropertyValue(DepartmentCodePKInfo, ref departmentCodePK, value);
				if (!IsValidationSuspended)
				{
					ValidateDepartmentCodePK();
				}
				DepartmentCodePKInfo.RefreshBinding();
			}
		}
		ZGuid departmentCodePK;

		public ZPropertyInfo DepartmentCodePKInfo
		{
			get { return GetZPropertyInfo(Schema.DepartmentCodePK, "DepartmentCode"); }
		}

		public void ValidateDepartmentCodePK()
		{
			DepartmentCodePKInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(DepartmentCodePKInfo);
			if (!DepartmentCodePKInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidPK(DepartmentCodePKInfo, DepartmentCodes);
			}
			if (!DepartmentCodePKInfo.HasErrors())
			{
				UniquePropertiesValidation.CheckPropertiesAreUniqueInCollection(BranchCodePKInfo, DepartmentCodePKInfo);
			}
		}
		#endregion

		#region ProfitCentre
		[MaxLength(10)]
		public ZString ProfitCentre
		{
			get { return profitCentre; }
			set
			{
				CheckMaximumLength(ProfitCentreInfo, value);
				SetNonPersistentPropertyValue(ProfitCentreInfo, ref profitCentre, value);
				if (!IsValidationSuspended)
				{
					ValidateProfitCentre();
				}
				ProfitCentreInfo.RefreshBinding();
			}
		}
		ZString profitCentre;

		public ZPropertyInfo ProfitCentreInfo
		{
			get { return GetZPropertyInfo(Schema.ProfitCentre); }
		}

		void ValidateProfitCentre()
		{
			ProfitCentreInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ProfitCentreInfo);
		}
		#endregion

		#region NominalDepartment
		[MaxLength(10)]
		public ZString NominalDepartment
		{
			get { return nominalDepartment; }
			set
			{
				CheckMaximumLength(NominalDepartmentInfo, value);
				SetNonPersistentPropertyValue(NominalDepartmentInfo, ref nominalDepartment, value);
				if (!IsValidationSuspended)
				{
					ValidateNominalDepartment();
				}
				NominalDepartmentInfo.RefreshBinding();
			}
		}
		ZString nominalDepartment;

		public ZPropertyInfo NominalDepartmentInfo
		{
			get { return GetZPropertyInfo(Schema.NominalDepartment); }
		}

		void ValidateNominalDepartment()
		{
			NominalDepartmentInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(NominalDepartmentInfo);
		}
		#endregion

		#region BusinessObject
		public GlbBranch CurrentBranch
		{
			get { return CurrentFactory.Load<GlbBranch>(BranchCodePK); }
		}

		public GlbBranchCollection BranchCodes
		{
			get { return branchCodes ?? (branchCodes = new GlbBranchCollection(CurrentFactory)); }
		}
		GlbBranchCollection branchCodes;

		public GlbDepartment CurrentDepartment
		{
			get { return CurrentFactory.Load<GlbDepartment>(DepartmentCodePK); }
		}

		public GlbDepartmentCollection DepartmentCodes
		{
			get { return departmentCodes ?? (departmentCodes = new GlbDepartmentCollection(CurrentFactory)); }
		}
		GlbDepartmentCollection departmentCodes;
		#endregion

		#region Write/Read XML
		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.BranchCodePK, BranchCodePK.ToString());
			writer.WriteElementString(Schema.DepartmentCodePK, DepartmentCodePK.ToString());
			writer.WriteElementString(Schema.ProfitCentre, ProfitCentre);
			writer.WriteElementString(Schema.NominalDepartment, NominalDepartment);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			BranchCodePK = new ZGuid(reader.ReadElementString(Schema.BranchCodePK));
			DepartmentCodePK = new ZGuid(reader.ReadElementString(Schema.DepartmentCodePK));
			ProfitCentre = reader.ReadElementString(Schema.ProfitCentre);
			NominalDepartment = reader.ReadElementString(Schema.NominalDepartment);
		}
		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BranchDepartmentCodeMappingRegistryBusinessObject(factory);
		}
	}
}
