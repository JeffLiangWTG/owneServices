using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Accounting.Business
{
	public class CreditControlledDocumentsApprovalDocument : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string DocumentName = "DocumentName";
		}

		#endregion

		public CreditControlledDocumentsApprovalDocument(ZString documentName)
		{
			this.documentName = documentName;
		}

		[ResourceStringData("CreditControlledDocumentsApprovalDocument|DocumentName", Caption = "Document Name")]
		public ZString DocumentName
		{
			get
			{
				return documentName;
			}
		}
		readonly ZString documentName;
	}
}