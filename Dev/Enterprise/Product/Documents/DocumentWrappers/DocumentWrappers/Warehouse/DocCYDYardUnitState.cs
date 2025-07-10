using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Yard.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocCYDYardUnitState : DocBaseWrapper
	{
		public DocCYDYardUnitState(CYDYardUnitState yardUnitState, BusinessObjectFactory factory) : base(yardUnitState, factory)
		{
		}

		public static DocCYDYardUnitState New(CYDYardUnitState yardUnitState, BusinessObjectFactory factoryToWrap)
		{
			return yardUnitState != null ? new DocCYDYardUnitState(yardUnitState, factoryToWrap) : null;
		}

		CYDYardUnitState YardUnitState => (CYDYardUnitState)WrappedObject;

		#region Properties

		public ZString Size
		{
			get { return YardUnitState.Container.RC_Length.ToString().Split('.')[0]; }
		}

		#endregion
	}
}
