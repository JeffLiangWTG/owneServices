using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.Common
{
	public class GenRegCertAccredMaintListValidation : MasterFiles.Business.GenRegCertAccredMaintListValidation
	{
		public GenRegCertAccredMaintListValidation(AutoGenRegCertAccredMaintList parent) : base(parent)
		{
		}

		protected override void CheckXZ_RefNumber()
		{
			if (Parent.XZ_Type == Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK)
			{
				if (Parent.XZ_RefNumber.IsEmpty)
				{
					Parent.XZ_RefNumberInfo.AddMessageError(MandatoryValidation.MustBeEnteredMessage(Parent.XZ_RefNumberInfo.Description));
				}
				else if (!Regex.IsMatch(Parent.XZ_RefNumber, "^[0-9A-Z]{5}$"))
				{
					Parent.XZ_RefNumberInfo.AddMessageError("Please enter exactly 5 uppercase alphanumeric characters.");
				}
			}
		}
	}
}
