using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class DocumentSendingObject : SupportingDocSendingObject
	{
		public DocumentSendingObject(CusExitReport exitReport) : base(exitReport)
		{
			cusExitReport = Argument.NotNull(exitReport, nameof(exitReport));
		}

		protected override SupportingDocSendingObjectValidation GetNewValidation() => new DocumentSendingObjectValidation(this);

		public new DocumentSendingObjectValidation Validation => (DocumentSendingObjectValidation)base.Validation;

		readonly CusExitReport cusExitReport;

		protected override IEnumerable<IStorageDocsBaseCollection> AllEDocsList
		{
			get
			{
				var list = ((IDocManagerSupportProvider)cusExitReport)?.DocManagerSupports.ToList() ?? new List<IDocManagerSupport>();
				return EDocsHelper.GetEDocCollections(list.ToArray());
			}
		}
	}
}
