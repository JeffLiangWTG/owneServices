using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public partial class JobDeclaration
	{
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.GatewayList))]
		public override ZString JE_Gateway
		{
			get => base.JE_Gateway;
			set => base.JE_Gateway = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.AuthorisationNumberList))]
		public override ZString JE_AuthorisationNumber
		{
			get => base.JE_AuthorisationNumber;
			set => base.JE_AuthorisationNumber = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.StyleOfEntrySOEList))]
		public override ZString JE_StyleOfEntrySOE
		{
			get => base.JE_StyleOfEntrySOE;
			set => base.JE_StyleOfEntrySOE = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.MethodOfPaymentList))]
		public override ZString JE_MethodOfPayment
		{
			get { return base.JE_MethodOfPayment; }
			set { base.JE_MethodOfPayment = value; }
		}
	}
}
