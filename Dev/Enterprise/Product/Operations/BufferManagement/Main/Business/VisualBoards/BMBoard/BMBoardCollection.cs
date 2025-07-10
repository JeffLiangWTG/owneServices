using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[ModuleID(ModuleId.BMBoard)]
	public class BMBoardCollection : ActiveBusinessObjectCollection<BMBoard>, IBMBoardCollection
	{
		public BMBoardCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public BMBoardCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}

		public BMBoardCollection(BMSystem system)
			: this(system, BMBoardFilterForCurrentUserProvider.GetQuery(system))
		{
			// TODO: make this constructor pass new ZQuery() up the constructor chain. We're using this here so that BMBoardFilterForCurrentUserProvider is not exposed when used in VisualBoards.Business. BMBoard[Section] should be moved to VisualBoards.Business
		}

		public BMBoardCollection(BMSystem system, ZQuery query)
			: base(system.Factory, system, query, BMBoardSchema.MB_FS_System)
		{
		}

		#region IBMBoardCollection Members

		IBMBoard IBMBoardCollection.this[int index]
		{
			get { return this[index]; }
		}

		#endregion
	}
}
