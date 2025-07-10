using CargoWise.Types;

using Enterprise.DocumentWrappers;

namespace Enterprise.Client.MFI.DocWrappers
{
	public class DocMFIBillofLadingContainer : DocBillofLadingContainer
	{
		#region Contructors and Type Overrides

		protected DocMFIBillofLadingContainer(DocContainer docContainer)
			: base(docContainer)
		{
		}

		public new static DocMFIBillofLadingContainer New(DocContainer docContainer)
		{
			return (docContainer != null) ? new DocMFIBillofLadingContainer(docContainer) : null;
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(OverriddenNewMethod);
		}

		static DocBillofLadingContainer OverriddenNewMethod(DocContainer docContainer)
		{
			return DocMFIBillofLadingContainer.New(docContainer);
		}

		#endregion

		public ZDecimal GrossWeight
		{
			get { return Container != null ? Container.GrossWeight : (ZDecimal)0m; }
		}
	}
}
