using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Customs.DE.Business
{
	public class CreateDeclarationBizObj : AutoNonPersistentCreateDeclarationBizObj
	{
		public CreateDeclarationBizObj(bool createFromWarehouseOrder) : base(new BusinessObjectFactory())
		{
			CreateFromWarehouseOrder = createFromWarehouseOrder;
		}

		public readonly bool CreateFromWarehouseOrder;

		public bool CanCreateDeclarations
		{
			get
			{
				RunPreSaveValidation();
				return !HasErrors && !HasMessageErrors;
			}
		}

		public string CreateDeclarationsForWarehouseOrder(IWhsOrder order)
		{
			Argument.NotNull(order, nameof(order));

			SelectedWhsOrder = order;

			var errorMsg = string.Empty;
			if (CreateFromWarehouseOrder && CanCreateDeclarations)
			{
				var orderLineToPickLines = BondedWarehousingHelper.GetPickLinesForOrderLines(Factory, order);
				if (orderLineToPickLines.All(x => x.Value.Count() == 1))
				{
					if (DeclarantsReference.IsEmpty)
					{
						DeclarantsReference = order.WD_ExternalReference.Left(DeclarantsReferenceInfo.MaxLength);
					}

					var inventorySelectionHeader = new InventorySelectionHeaderForDeclarationCreation(this, updateWarehouse: false);

					var wrappers = new List<WhsInventoryWrapper>();
					foreach (var entry in orderLineToPickLines)
					{
						var orderLine = entry.Key;
						var pickLine = entry.Value.Single();

						var receiveLine = BondedWarehousingHelper.GetReceiveLineForPickLine(Factory, pickLine);
						wrappers.Add(new WhsInventoryWrapper(receiveLine.Inventory, inventorySelectionHeader) { QuantityToDraw = pickLine.WZ_Units, Order = order, OrderLines = new[] { orderLine } } );
					}

					inventorySelectionHeader.SelectedLines.AddRange(wrappers);
					inventorySelectionHeader.ImportInventories();
				}
				else
				{
					var orderWithMultiplePickLines = orderLineToPickLines.First(x => x.Value.Count() != 1).Key;
					errorMsg = Res.GetString("05F3F8C9-62CA-420C-948A-1D502F3544B3", "You cannot import order {0} with multiple picks on order line {1}", order.WD_ExternalReference, orderWithMultiplePickLines.WE_LineNo);
				}
			}
			return errorMsg;
		}

		public IWhsOrder SelectedWhsOrder { get; private set; }

		[List(nameof(Lookups) + "." + nameof(CreateDeclarationBizObjLookups.DeclarationTypeList))]
		public override ZString DeclarationType { get => base.DeclarationType; set => base.DeclarationType = value; }

		[List(nameof(Lookups) + "." + nameof(CreateDeclarationBizObjLookups.CpcList))]
		public override ZString CPC { get => base.CPC; set => base.CPC = value; }

		[List(nameof(Lookups) + "." + nameof(CreateDeclarationBizObjLookups.CustomsOffices))]
		public override ZString CustomsOffice { get => base.CustomsOffice; set => base.CustomsOffice = value; }

		public CreateDeclarationBizObjLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = GetNewLookups();
				}

				return fLookups;
			}
		}

		protected virtual CreateDeclarationBizObjLookups GetNewLookups()
		{
			return new CreateDeclarationBizObjLookups(this);
		}

		CreateDeclarationBizObjLookups fLookups;

		protected override NonPersistentCreateDeclarationBizObjValidation GetNewValidation() => new NonPersistentCreateDeclarationBizObjValidation(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			DeclarationType = ImportDeclarationTypeList.Codes.EZA;
			CPC = ImportMainProcedureCodeList.Codes._40;
		}
	}
}
