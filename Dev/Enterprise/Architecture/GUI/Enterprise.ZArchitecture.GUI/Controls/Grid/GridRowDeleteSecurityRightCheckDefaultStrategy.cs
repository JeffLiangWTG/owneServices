// -----------------------------------------------------------------------
// <copyright file="Class1.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Enterprise.ZArchitecture.GUI
{
	using CargoWise.EntityFramework;
	using Enterprise.Security;
	using Enterprise.ZArchitecture.Modules;

	public interface IGridRowDeleteSecurityRightCheckStrategy
	{
		void ProcessDeleteRequest(RowsDeletingEventArgs args, ZController controller);
	}

	public class GridRowSecurityRightCheckDefaultStrategy : IGridRowDeleteSecurityRightCheckStrategy
	{
		void IGridRowDeleteSecurityRightCheckStrategy.ProcessDeleteRequest(RowsDeletingEventArgs args, ZController controller)
		{
			if (controller != null && ShouldCheckSecurityRight(args))
			{
				var checkPoint = GetCheckPoint(controller, null);
				if (checkPoint != null && !checkPoint.IsAllowed)
				{
					args.Cancel = true;
					checkPoint.ShowError();
				}
			}
		}

		protected virtual bool ShouldCheckSecurityRight(RowsDeletingEventArgs args)
		{
			return true;
		}

		public virtual SecurityCheckpoint GetCheckPoint(ZController controller,BusinessObject bizObject)
		{
			return null;
		}
	}

	public class GridRowDeleteSecurityRightCheckDefaultStrategy : GridRowSecurityRightCheckDefaultStrategy
	{
		public override SecurityCheckpoint GetCheckPoint(ZController controller,BusinessObject bizObject)
		{
			return controller.GetCheckPointForDelete(null);
		}
	}

	public class GridRowRemoveSecurityRightCheckDefaultStrategy : GridRowSecurityRightCheckDefaultStrategy
	{
		public override SecurityCheckpoint GetCheckPoint(ZController controller,BusinessObject bizObject)
		{
			return controller.GetCheckPointForEdit(null);
		}
	}
}
