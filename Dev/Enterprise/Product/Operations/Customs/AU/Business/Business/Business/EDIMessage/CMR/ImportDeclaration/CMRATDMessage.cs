using System.Data;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRATDMessage : CMRImportDeclarationMessage
	{
		public CMRATDMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region AuthorityToDeal

		public AuthorityToDeal AuthorityToDeal
		{
			get
			{
				if (fAuthorityToDeal == null)
				{
					fAuthorityToDeal = new AuthorityToDeal(CUSRES);
				}

				return fAuthorityToDeal;
			}
		}
		AuthorityToDeal fAuthorityToDeal;

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.ATD;
		}

		public ZString GetATDSecurityCode()
		{
			return AuthorityToDeal.SecurityCode;
		}

		public override ZString GetReport()
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(base.GetReport() + "\r\n");
			builder.Append("Authority to Deal Date Issued: " + AuthorityToDeal.AuthorityToDealDateIssued + "\r\n");
			builder.Append("Payment Finalised Date: " + AuthorityToDeal.PaymentFinalisedDateString + "\r\n");
			builder.Append("Authority to Deal Security Code: " + AuthorityToDeal.SecurityCode + "\r\n");

			if (!AuthorityToDeal.AuthorityToDealActionReason.IsEmpty)
			{
				builder.Append("\r\nAuthority to Deal Action Reason: " + AuthorityToDeal.AuthorityToDealActionReason + "\r\n");
			}

			return builder.ToString();
		}

		public override ZString StatusOfLinesReport
		{
			get { return ZString.Empty; }
		}
	}
}
