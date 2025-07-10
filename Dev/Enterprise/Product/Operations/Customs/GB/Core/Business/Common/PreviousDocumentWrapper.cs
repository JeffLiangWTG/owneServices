using System;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Integration.SadH;

namespace Enterprise.Customs.GB.Business.Messaging
{
	public class PreviousDocumentWrapper : IPreviousDocument
	{
		public PreviousDocumentWrapper(PreviousDocument previousDocument)
		{
			this.previousDocument = previousDocument ?? throw new ArgumentNullException(nameof(previousDocument));
		}
		protected readonly PreviousDocument previousDocument;

		#region IPreviousDocument Members

		CargoWise.Types.ZString IPreviousDocument.Class => previousDocument.CSI_SubType;

		CargoWise.Types.ZString IPreviousDocument.Reference
		{
			get => previousDocument.CSI_ReferenceNumber + (previousDocument.CSI_DateOfIssue.IsValid ? "-" + previousDocument.DateOfIssueInFormat : string.Empty);
		}

		CargoWise.Types.ZString IPreviousDocument.Type => previousDocument.CSI_Code;

		CargoWise.Types.ZString IPreviousDocument.MoreInfo => previousDocument.CSI_Description;

		#endregion
	}
}
