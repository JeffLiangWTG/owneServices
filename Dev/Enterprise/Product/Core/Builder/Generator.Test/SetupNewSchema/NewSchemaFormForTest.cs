using System;
using CargoWise.Data;

namespace Enterprise.Builder.Generator.Test
{
	sealed class NewSchemaFormForTest : NewSchemaForm
	{
		public void OnControllerThreadStart_Exposed()
		{
			base.OnControllerThreadStart();
		}

		internal protected override Controller ControllerFactory(IProgressLogger logger)
		{
			_ = Db.Connection;
			throw new InvalidOperationException("throw exception from ControllerFactory");
		}
	}
}
