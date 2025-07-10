using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared.JobComInvHeaderCharge;

namespace Enterprise.Customs.Common
{
	public interface IJobComInvApportionedChargeCollection<out T> : IJobComInvChargeBaseCollection<T>
		where T : JobComInvCharge
	{
		new T this[int index] { get; }
		T FirstOrDefault();
		T AddNewAMMVCharge();

		void Rebuild();
		void ClearApportionedCharges();
		void DeleteEmptyApportionedCharges();
		void SetReadOnlyIncludingChildren(bool readOnly);
		void UpdatePrepaidCollect(ChargeCodeChargeKey chargeKey, string prepaidCollect);
	}

	public class JobComInvApportionedChargeCollection<T> : JobComInvChargeCollectionBase<T>, IJobComInvApportionedChargeCollection<T>
		where T : JobComInvCharge
	{
		public JobComInvApportionedChargeCollection(ICommonInvoice invoice)
			: base(invoice)
		{
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pk) => typeOfElements ?? (typeOfElements = GetElementTypeFromCollectionType(GetType()));
		Type typeOfElements;

		public IEnumerator<T> GetEnumerator() => Elements.Cast<T>().GetEnumerator();

		public T FirstOrDefault() => this.Cast<T>().FirstOrDefault();

		public new T[] Find(ApportionChargeKey chargeCode) => base.Find(chargeCode).ToArray();

		public void ClearApportionedCharges()
		{
			foreach (var charge in this)
			{
				if (!charge.J7_IsSystem)
				{
					charge.J7_Amount = 0m;
					charge.J7_RX_NKCurrency = "";
					charge.J7_Percentage = 0m;
				}
			}
		}

		public void DeleteEmptyApportionedCharges()
		{
			var removableCharge = new List<JobComInvCharge>();
			foreach (var charge in this)
			{
				if (charge.IsEmpty && !charge.J7_IsSystem)
				{
					removableCharge.Add(charge);
				}
			}
			removableCharge.ForEach(RemoveAndDelete);
		}

		public void UpdatePrepaidCollect(ChargeCodeChargeKey chargeKey, string prepaidCollect)
		{
			var charges = Find(chargeKey);
			foreach (var charge in charges)
			{
				charge.J7_PrepaidCollect = prepaidCollect;
			}
		}

		protected override bool AllowNewCore => false;

		public override bool ReadOnly => true;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var charge = (JobComInvCharge)element;
			return charge != null && charge.J7_IsApportionedCharge;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var charge = (T)child;
			charge.J7_IsApportionedCharge = true;
		}
	}
}
