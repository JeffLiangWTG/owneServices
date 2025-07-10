using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ContainerSelectionBusinessObject : NonPersistentBusinessObject
	{
		public ContainerSelectionBusinessObject(ContainerSelectionBusinessObjectCollection parentCollection)
		{
			this.parentCollection = parentCollection;
		}

		readonly ContainerSelectionBusinessObjectCollection parentCollection;

		#region IsSelected

		public ZBool IsSelected
		{
			get => isSelected;
			set
			{
				if (isSelected != value)
				{
					SetNonPersistentPropertyValue(IsSelectedInfo, ref isSelected, value);
					parentCollection.OnSelectionsChanged();
				}
			}
		}
		ZBool isSelected;

		public ZPropertyInfo IsSelectedInfo => GetZPropertyInfo(nameof(IsSelected));

		#endregion

		#region Container Number

		public ZString Number
		{
			get => containerNumber;
			set => SetNonPersistentPropertyValue(NumberInfo, ref containerNumber, value);
		}
		ZString containerNumber;

		public ZPropertyInfo NumberInfo => GetZPropertyInfo(nameof(Number));

		#endregion

		#region Container Weight

		[DecimalPlaces(3)]
		public ZDecimal Weight
		{
			get => containerWeight;
			set => SetNonPersistentPropertyValue(WeightInfo, ref containerWeight, value);
		}
		ZDecimal containerWeight;

		public ZPropertyInfo WeightInfo => GetZPropertyInfo(nameof(Weight));

		#endregion

		#region Container Volume

		[DecimalPlaces(3)]
		public ZDecimal Volume
		{
			get => containerVolume;
			set => SetNonPersistentPropertyValue(VolumeInfo, ref containerVolume, value);
		}
		ZDecimal containerVolume;

		public ZPropertyInfo VolumeInfo => GetZPropertyInfo(nameof(Volume));

		#endregion

		#region Container Comodity

		public ZString Commodity
		{
			get => containerCommodity;
			set => SetNonPersistentPropertyValue(CommodityInfo, ref containerCommodity, value);
		}
		ZString containerCommodity;

		public ZPropertyInfo CommodityInfo => GetZPropertyInfo(nameof(Commodity));

		#endregion

		#region Quantity

		public ZDecimal Quantity
		{
			get => quantity;
			set => SetNonPersistentPropertyValue(QuantityInfo, ref quantity, value);
		}
		ZDecimal quantity;

		public ZPropertyInfo QuantityInfo => GetZPropertyInfo(nameof(Quantity));

		#endregion
	}

	public class ContainerSelectionBusinessObjectCollection : NonPersistentBusinessObjectCollection<ContainerSelectionBusinessObject>
	{
		public void AddNewSelection(bool isSelected, ZString containerNumber, ZDecimal quantity, ZDecimal containerWeight, ZDecimal containerVolume, ZString commodity)
		{
			var containerSelection = AddNew();
			containerSelection.IsSelected = isSelected;
			containerSelection.Number = containerNumber;
			containerSelection.Quantity = quantity;
			containerSelection.Weight = containerWeight;
			containerSelection.Volume = containerVolume;
			containerSelection.Commodity = commodity;
		}

		public ZDecimal TotalQuantity => this.Cast<ContainerSelectionBusinessObject>().Sum(c => c.Quantity);

		public ZDecimal SelectedQuantity => SelectedContainers.Sum(c => c.Quantity);

		public ZString SelectedNumbers => string.Join(", ", SelectedContainers.Select(c => c.Number));

		IEnumerable<ContainerSelectionBusinessObject> SelectedContainers => this.Cast<ContainerSelectionBusinessObject>().Where(c => c.IsSelected);

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ContainerSelectionBusinessObject(this);
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		public ContainerSelectionBusinessObjectCollection Clone()
		{
			var newCollection = new ContainerSelectionBusinessObjectCollection();
			foreach (ContainerSelectionBusinessObject item in this)
			{
				newCollection.AddNewSelection(item.IsSelected, item.Number, item.Quantity, item.Weight, item.Volume, item.Commodity);
			}

			return newCollection;
		}

		public void OnSelectionsChanged()
		{
			SelectionsChanged?.Invoke(this, new EventArgs());
		}

		public event EventHandler SelectionsChanged;
	}
}

