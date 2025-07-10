using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class JobDeclarationCustomsCharges : Customs.Business.InterfaceImplementations.JobDeclarationCustomsCharges
	{
		public JobDeclarationCustomsCharges(JobDeclaration declaration)
			: base(declaration)
		{
		}

		#region ICustomsCharges Members

		protected override ZBool IsCustomsChargesActiveCore
		{
			get { return declaration.JE_MessageType != JobMessageTypeList.Codes.Export && declaration.JE_MessageType != JobMessageTypeList.Codes.MiscellaneousCustoms; }
		}

		#endregion
	}
}
