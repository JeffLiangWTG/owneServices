using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ExitControlBase.Business;

namespace Enterprise.Customs.CH.DeclarationActivation.Business;

public class DeclarationActivationConsignment(BusinessObjectFactory factory, DataRow row) : CusExitConsignment(factory, row)
{
	new class Schema : AutoCusExitConsignment.Schema
	{
		public new const int CXC_ReferenceNumberMaxLength = 11;
		public new const int CXC_MovementReferenceMaxLength = 21;
	}

	[MaxLength(Schema.CXC_ReferenceNumberMaxLength)]
	public override ZString CXC_ReferenceNumber { get => base.CXC_ReferenceNumber; set => base.CXC_ReferenceNumber = value; }

	[MaxLength(Schema.CXC_MovementReferenceMaxLength)]
	public override ZString CXC_MovementReference { get => base.CXC_MovementReference; set => base.CXC_MovementReference = value; }
}
