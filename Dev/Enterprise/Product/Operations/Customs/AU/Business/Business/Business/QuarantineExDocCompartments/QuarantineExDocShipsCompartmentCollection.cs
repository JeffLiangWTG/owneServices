using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineExDocShipsCompartmentCollection : DependentBusinessObjectCollection<QuarantineExDocShipsCompartment, QuarantineExDocHeader>
	{
		public QuarantineExDocShipsCompartmentCollection(QuarantineExDocHeader header)
			: base(header)
		{
			MaxCountValidationEnable(4);
		}

		public QuarantineExDocShipsCompartment FindByCompartmentNumber(ZString compartments)
		{
			QuarantineExDocShipsCompartment result = null;

			foreach (QuarantineExDocShipsCompartment currentCompartment in this)
			{
				if (currentCompartment.QC_Compartments == compartments)
				{
					result = currentCompartment;
					break;
				}
			}

			return result;
		}

		#region Cloning Stuff
		public void Clone(QuarantineExDocShipsCompartmentCollection collectionToClone)
		{
			RemoveAndDeleteAll();
			foreach (QuarantineExDocShipsCompartment compartment in collectionToClone)
			{
				QuarantineExDocShipsCompartment clonedCompartment = (QuarantineExDocShipsCompartment)compartment.Clone();
				using (clonedCompartment.SuspendSettingHasChanges())
				using (clonedCompartment.GetValidationSuspender())
				{
					((IBusinessObjectInternals)clonedCompartment).IsCopying = true;
					try
					{
						Add(clonedCompartment);
					}
					finally
					{
						((IBusinessObjectInternals)clonedCompartment).IsCopying = false;
					}
				}
			}
		}
		#endregion
	}
}
