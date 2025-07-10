using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Licensing
{
	public class LicenceUsageLog : StmActivityLog
	{
		public LicenceUsageLog(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			S7_MouseClicks = (int)ModuleLicenceType.NON;
		}

		public ZDateTime LocalUsageTime
		{
			get
			{
				ZDateTime result = S7_OpenDateTimeUtc;
				if (!result.IsEmpty)
				{
					result = Env.Time.GetLocalTimeFromUtc(result.ToDateTime());
				}
				return result;
			}
		}

		public ZPropertyInfo LocalUsageTimeInfo
		{
			get { return GetZPropertyInfo(nameof(LocalUsageTime)); }
		}

		#region Licence Name

		public ZString LicenceModuleDescription
		{
			get
			{
				ILicenceCheckpoint checkpoint = EnvProxy.Instance.Licence.GetCheckpointFromCode(S7_FormCaption);
				return checkpoint != null ? checkpoint.DisplayName : "";
			}
		}

		public ZPropertyInfo LicenceModuleDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(LicenceModuleDescription)); }
		}

		#endregion		

		public string LicenceType
		{
			get
			{
				int mouseClicks = S7_MouseClicks;
				return ((ModuleLicenceType)mouseClicks).ToString();
			}
		}

		[CargoWise.ComponentModel.MaxLength(50)]
		public ZString LicenceTypeDescription
		{
			get { return LicenceTypes.GetDescriptionFromCode(LicenceType); }
		}

		public ZPropertyInfo LicenceTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(LicenceTypeDescription)); }
		}

		LicenceTypes LicenceTypes
		{
			get { return licenceTypes ?? (licenceTypes = new LicenceTypes()); }
		}

		LicenceTypes licenceTypes;

		public GlbBranch ParentBranch
		{
			get { return S7_ParentTableCode == GlbBranchSchema.Constants.Prefix ? Factory.Load<GlbBranch>(S7_ParentID) : null; }
		}
	}
}
