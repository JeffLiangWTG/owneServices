using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business
{
	public class JobComInvoiceHeaderContractCollection : ActiveBusinessObjectCollection<JobComInvoiceHeaderContract>
	{
		public JobComInvoiceHeaderContractCollection(JobComInvoiceHeader parent)
			: base(parent, RelationshipFilter)
		{
		}

		static ZQuery RelationshipFilter => new ZQuery(JobComInvoiceHeaderRefsSchema.J2_ReferenceType, JobComInvoiceHeaderContract.Constants.CTR);

		public ZString ContractNumbersAsString
		{
			get
			{
				return this.Select(x => x.J2_ReferenceNumber).JoinAsString();
			}
			set
			{
				if (ContractNumbersAsString != value)
				{
					using (SuppressContractNumbersChangedEvents())
					{
						DeleteAll();

						value.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ForEach(x =>
						{
							AddNew(x);
						});
					}
					OnContractNumbers_ValueChanged(this, EventArgs.Empty);
				}
			}
		}

		void AddNew(ZString number)
		{
			var numberValue = number.Trim();
			if (!numberValue.IsEmpty)
			{
				var contract = AddNew();
				contract.J2_ReferenceNumber = numberValue.Left(contract.J2_ReferenceNumberInfo.MaxLength);
			}
		}

		#region Event handling

		protected override void OnLoadedIntoCollectionCore(JobComInvoiceHeaderContract loadedObject)
		{
			base.OnLoadedIntoCollectionCore(loadedObject);
			loadedObject.J2_ReferenceNumberInfo.ValueChanged -= new EventHandler(OnContractNumbers_ValueChanged);
			loadedObject.J2_ReferenceNumberInfo.ValueChanged += new EventHandler(OnContractNumbers_ValueChanged);
		}

		protected override void OnAdded(JobComInvoiceHeaderContract businessObject)
		{
			base.OnAdded(businessObject);
			businessObject.J2_ReferenceNumberInfo.ValueChanged -= new EventHandler(OnContractNumbers_ValueChanged);
			businessObject.J2_ReferenceNumberInfo.ValueChanged += new EventHandler(OnContractNumbers_ValueChanged);
			OnContractNumbers_ValueChanged(this, EventArgs.Empty);
		}

		public override void Delete(JobComInvoiceHeaderContract businessObject)
		{
			base.Delete(businessObject);
			businessObject.J2_ReferenceNumberInfo.ValueChanged -= new EventHandler(OnContractNumbers_ValueChanged);
			OnContractNumbers_ValueChanged(this, EventArgs.Empty);
		}

		void OnContractNumbers_ValueChanged(object sender, EventArgs e)
		{
			if (!IsContractNumbersChangedEventsSuspended)
			{
				ContractNumbersChanged?.Invoke(sender, e);
			}
		}

		public event EventHandler ContractNumbersChanged;

		#endregion

		#region Contract Numbers Changed Events Suspender

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		int contractNumbersChangedEventsSuspender;

		bool IsContractNumbersChangedEventsSuspended => contractNumbersChangedEventsSuspender != 0;

		IDisposable SuppressContractNumbersChangedEvents()
		{
			contractNumbersChangedEventsSuspender++;
			return new DisposableAction(delegate
			{ contractNumbersChangedEventsSuspender--; });
		}

		#endregion

		#region Suppressed Overrides
#if DEBUG
		protected override void SetHasChanges(bool hasChanges)
		{
		}
#endif
		#endregion
	}
}
