
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DocumentStatusAndDocumentStatusConditions : IDocumentStatus, IDocumentStatusConditions, ICAN
	{
		#region IDocumentStatus Members

		public CMRDocumentStatus DocumentStatus
		{
			get
			{
				return fDocumentStatus;
			}
			set
			{
				fDocumentStatus = value;
			}
		}
		protected CMRDocumentStatus fDocumentStatus;

		#endregion

		#region IDocumentStatusConditions Members

		public CMRDocumentStatusConditions DocumentStatusConditions
		{
			get
			{
				return fDocumentStatusConditions;
			}
			set
			{
				fDocumentStatusConditions = value;
			}
		}
		protected CMRDocumentStatusConditions fDocumentStatusConditions;

		#endregion

		#region ICAN Members

		public ZString CAN
		{
			get
			{
				return fCAN;
			}
			set
			{
				fCAN = value;
			}
		}
		protected ZString fCAN;

		#endregion
	}
}
