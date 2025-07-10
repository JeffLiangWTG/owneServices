using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.MX.Business
{
	public class GenRegCertAccredMaintListValidation : MasterFiles.Business.GenRegCertAccredMaintListValidation
	{
		public GenRegCertAccredMaintListValidation(AutoGenRegCertAccredMaintList parent) : base(parent)
		{
		}

		protected override void CheckXZ_RefNumber()
		{
			base.CheckXZ_RefNumber();

			if (Parent.XZ_RN_NKCountryOfIssuance == Core.Constants.CountryCodes.Mexico && Parent.XZ_Type == Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK)
			{
				var refNumber = Parent.XZ_RefNumber;
				var refNumberInfo = Parent.XZ_RefNumberInfo;

				if (refNumber.IsEmpty)
				{
					refNumberInfo.AddWarning(Res.GetString("C0CDD8C4-0D56-4171-BC66-18F7B5EDE7E3", "The Broker’s Patent Number that allows their operation in Customs Clearance Areas is mandatory."));
				}
				else if (refNumber.Length != 4)
				{
					Parent.XZ_RefNumberInfo.AddError(Res.GetString("71C88177-EE9B-4326-B457-D4E0770F40B5", "A Patent number must be 4 digits long."));
				}
				else
				{
					var query = new ZQuery(GenRegCertAccredMaintListSchema.XZ_RefNumber, refNumber);
					query.AddToFilter(GenRegCertAccredMaintListSchema.XZ_Type, Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK);
					query.AddToFilter(GenRegCertAccredMaintListSchema.XZ_RN_NKCountryOfIssuance, Core.Constants.CountryCodes.Mexico);

					if (Parent.Factory.Load<GenRegCertAccredMaintList>(query).Length > 1)
					{
						Parent.XZ_RefNumberInfo.AddError(Res.GetString("29E68780-BB46-48E5-810C-F3FD120CCBEC", "A Patent number must be unique per Staff."));
					}
				}
			}
		}

		protected override void CheckXZ_Type()
		{
			base.CheckXZ_Type();

			if (Parent.XZ_RN_NKCountryOfIssuance == Core.Constants.CountryCodes.Mexico)
			{
				var genRegCertAccredMaintList = Parent as GenRegCertAccredMaintList;
				var glbStaff = genRegCertAccredMaintList.MasterParent as GlbStaff;

				if (glbStaff != null)
				{
					if (glbStaff.Certificates.Cast<GenRegCertAccredMaintList>().Count(w => w.XZ_Type == Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK && w.XZ_RN_NKCountryOfIssuance == Core.Constants.CountryCodes.Mexico) > 1)
					{
						Parent.XZ_TypeInfo.AddError(Res.GetString("63E045B3-0834-4500-8009-54AA4F0B48D2", "A Broker (BRK) Type must be unique per Staff"));
					}
				}
			}
		}
	}
}
