using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public class HiddenStmNoteNotAutoLogged : HiddenStmNote
	{
		public HiddenStmNoteNotAutoLogged(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override EnterpriseBusinessObject.AutologState AutoLoggingState => EnterpriseBusinessObject.AutologState.NotLogged;
	}
}
