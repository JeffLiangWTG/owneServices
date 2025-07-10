using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;

namespace Enterprise.Client.UPE.Business.Testing
{
	public class CusHAWBWithDummyDeclarationChargesForTest : Callout
	{
		public CusHAWBWithDummyDeclarationChargesForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public void SetCustomsCharges(params CustomsCharge[] customsCharges)
		{
			UPEJobDeclarationWithDummyCharges declaration = (UPEJobDeclarationWithDummyCharges)this.Declaration;
			declaration.SetCustomsCharges(customsCharges);
		}

		public override UPEJobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<UPEJobDeclarationWithDummyCharges>();
				}

				return fDeclaration;
			}
		}

		UPEJobDeclaration fDeclaration;
	}
}
