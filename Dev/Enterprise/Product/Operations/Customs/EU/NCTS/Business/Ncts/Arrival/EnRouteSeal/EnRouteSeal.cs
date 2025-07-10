using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class EnRouteSeal : CusInBondEvent, Integration.Customs.EU.NCTS.IEnRouteSeal, ICusInBondContainerTypeSupporter
	{
		public EnRouteSeal(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new NctsHeader Header => (NctsHeader)base.Header;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BN_Type = CusInBondEventTypeList.Codes.Seal;
		}

		[List(nameof(Lookups) + "." + nameof(EnRouteSealLookups.EventCountries))]
		[ResourceStringData("54E6AB22-3A86-44B9-92D7-47EB4746C437", Caption = "Event Country")]
		public override ZString BN_EventCountryCode { get => base.BN_EventCountryCode; set => base.BN_EventCountryCode = value; }

		SealContainerCollection sealContainers;
		[ChildEditable]
		public SealContainerCollection SealContainers
		{
			get
			{
				if (sealContainers == null)
				{
					sealContainers = new SealContainerCollection(this);
					sealContainers.Load();
					RegisterEditableChildObject(sealContainers);
				}
				return sealContainers;
			}
		}

		Type ICusInBondContainerTypeSupporter.ContainerType => typeof(SealContainer);

		public IEnumerable<ZString> SealContainersSealNumbers
		{
			get
			{
				var numbers = new List<ZString>();
				foreach (SealContainer container in SealContainers)
				{
					if (!container.BC_Seal1.IsEmpty)
					{
						numbers.Add(container.BC_Seal1);
					}
				}
				return numbers;
			}
		}

		public new EnRouteSealLookups Lookups => (EnRouteSealLookups)base.Lookups;

		protected override CusInBondEventLookups GetNewLookups() => new EnRouteSealLookups(this);

		public new EnRouteSealValidation Validation => (EnRouteSealValidation)base.Validation;

		protected override CusInBondEventValidation GetNewValidation() => new EnRouteSealValidation(this);
	}
}
