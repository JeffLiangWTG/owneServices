using System.Data;

using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Client.WCB
{
	internal class JobDeclarationWithFixedInvHeads : JobDeclaration
	{
		public JobDeclarationWithFixedInvHeads(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ChildEditable]
		public InvHeadFixedCollection FixedInvoices
		{
			get
			{
				if (fixedInvoices == null)
				{
					fixedInvoices = new InvHeadFixedCollection(this);
					RegisterEditableChildObject(fixedInvoices);
				}
				return fixedInvoices;
			}
		}
		InvHeadFixedCollection fixedInvoices;
	}
}
