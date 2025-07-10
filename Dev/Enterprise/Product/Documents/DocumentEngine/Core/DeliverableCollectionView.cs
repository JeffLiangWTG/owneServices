using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine
{
	public class DeliverableCollectionView : BusinessObjectCollectionView<BusinessObject>
	{
		public DeliverableCollectionView(IDeliverableCollection deliverables, DocDeliveryContactCollection recipients)
			: base((BusinessObjectCollection)deliverables)
		{
			this.Recipients = recipients;
			Rebuild();
		}

		public new IDeliverable this[int index]
		{
			get { return (IDeliverable)Elements[index]; }
		}

		protected override void RebuildOnConstruction()
		{
			// don't rebuild yet as we have not set the Recipients 
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowSort
		{
			get { return false; }
		}

		#region IsPrintable 

		// This method will ensure that if the current delivery medium is printing, that ONLY those documents/reports/eDocs that can be printed are "Included" for printing.  However
		// if the medium is emailing, then only documents and reports can be previewed, eventhough eDocs are "Included" for delivery.
		protected override void RebuildCore()
		{
			using (SuppressRebuild())
			{
				base.RebuildCore();

				var shouldValidateDeliveryMethod = false;
				using (SuspendRecipientsValidation())
				{
					foreach (var element in this)
					{
						var deliverable = (IDeliverable)element;
						var includeInPrint = IsDeliveryModeSupportedByRecipients(deliverable);
						if (!includeInPrint)
						{
							shouldValidateDeliveryMethod = true;
							deliverable.IncludedInPrint = false;
							deliverable.CanIncludeInPrint = false;
						}
						else
						{
							if (!deliverable.CanIncludeInPrint)
							{
								shouldValidateDeliveryMethod = true;
								deliverable.IncludedInPrint = deliverable.ShouldPrintByDefault;
								deliverable.CanIncludeInPrint = true;
							}
						}
						deliverable.IncludedInPrint_ReadOnly = !IsDeliverableEditable(deliverable);
					}
				}

				if (shouldValidateDeliveryMethod)
				{
					ValidateDeliveryMethodForRecipients();
				}
			}
		}

		protected IDisposable SuspendRecipientsValidation()
		{
			var recipients = Recipients?.OfType<DocDeliveryContact>().ToArray();
			return new DisposableAction(() => { recipients?.ForEach(r => r.SuspendValidation()); this.ForEach(d => d.SuspendValidation()); },
				() => { recipients?.ForEach(r => r.ResumeValidation()); this.ForEach(d => d.ResumeValidation()); });
		}

		bool IsDeliveryModeSupportedByRecipients(IDeliverable deliverable)
		{
			return Recipients == null || Recipients.Count == 0 || Recipients.OfType<DocDeliveryContact>().Any(contact => deliverable.SupportsDeliveryMethod(contact.DeliveryMethod));
		}

		bool IsDeliverableEditable(IDeliverable deliverable)
		{
			return Recipients == null || Recipients.Count == 0 || Recipients.OfType<DocDeliveryContact>().Any(contact => deliverable.GetSupportedDeliveryMethodDespiteOfPrintCopyType().Contains<string>(contact.DeliveryMethod));
		}

		#endregion

		#region Collection Filtering

		public bool HasIncludedDocuments
		{
			get
			{
				foreach (IDeliverable deliverable in this)
				{
					if (deliverable.IncludedInPrint)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool HasIncludedPreviewableDocuments
		{
			get
			{
				foreach (var deliverable in this.OfType<IDeliverable>().Where(deliverable => deliverable.IncludedInPrint))
				{
					var format = deliverable.GetDeliveryInfo(false).DeliveryFormat;
					if (format == DeliveryMethods.DeliveryInfo.DeliveryFormats.Document || format == DeliveryMethods.DeliveryInfo.DeliveryFormats.Report)
					{
						return true;
					}
				}
				return false;
			}
		}

		// All documents should be visible.  Whether or not they print is now dependant upon the IDeliverable.IncludedInPrint property.
		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return true;
		}

		internal DocDeliveryContactCollection Recipients;

		public void ValidateDeliveryMethodForRecipients()
		{
			Recipients?.OfType<DocDeliveryContact>()?.ToArray()?.ForEach(r => r.Validation.ValidateDeliveryMethod());
		}

		#endregion
	}
}
