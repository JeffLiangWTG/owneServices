using System.ComponentModel;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSMessageSendingAction : BaseMessageSendingObject
	{
		public EMCSMessageSendingAction(EMCSJobDeclaration declaration) : base(declaration.Factory)
		{
			jobDeclaration = Argument.NotNull(declaration, nameof(declaration));
			ShouldSend = true;
		}
		readonly EMCSJobDeclaration jobDeclaration;

		[ResourceStringData("FC84D639-A78D-42FD-B9C3-E6681CDE965F", Caption = "Declarant Type")]
		[ReadOnly(true)]
		public ZString DeclarantType => jobDeclaration.Lookups.DeclarantTypeList.GetDescriptionFromCode(jobDeclaration.JE_DeclarantType);

		public ZPropertyInfo DeclarantTypeInfo => GetZPropertyInfo(nameof(DeclarantType));

		[ResourceStringData("91132EA2-58B2-43E6-9901-6A8DB91D80E7", Caption = "EAD Number")]
		public ZString EADNumber => jobDeclaration.EADNumber;

		public ZPropertyInfo EADNumberInfo => GetZPropertyInfo(nameof(EADNumber));

		[ResourceStringData("75E27FDA-083E-4293-A2DA-907037EAC543", Caption = "Registration Status")]
		public ZString RegistrationStatus => jobDeclaration.JE_EntryStatus;

		public ZPropertyInfo RegistrationStatusInfo => GetZPropertyInfo(nameof(RegistrationStatus));

		protected override bool ShouldSend_ReadOnly => true;
	}
}
