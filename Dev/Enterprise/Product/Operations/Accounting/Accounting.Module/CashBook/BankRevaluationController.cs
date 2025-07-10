#if YouHaveMadeTheBankModule
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Enterprise.ZArchitecture;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.GUI;
using Enterprise.Core.Environment;
using Enterprise.Accounting.Business.CashBook;
//using Enterprise.Accounting.GUI.CashBook.Revaluation;

namespace Enterprise.Accounting.Module
{
	/// <summary>
	/// Module Controller for CashBook.
	/// </summary>
	public class BankRevaluationController : ZController
	{
		public BankRevaluationController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.BankRevaluation; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(BankRevaluation); }
		}

		protected override IZForm GetForm(IBusiness BusinessEntity)
		{
			return new BankRevaluationForm((BankRevaluation)BusinessEntity);
		}

		protected override Enterprise.Core.Environment.SecurityCheckpoint CheckPointForNew
		{
			get	{ return Env.Security.None; }
		}

		protected override Enterprise.Core.Environment.SecurityCheckpoint CheckPointForEdit
		{
			get	{ return Env.Security.None; }
		}

		protected override Enterprise.Core.Environment.SecurityCheckpoint CheckPointForView
		{
			get	{ return Env.Security.None; }
		}

		protected override Enterprise.Core.Environment.SecurityCheckpoint CheckPointForDelete
		{
			get	{ return Env.Security.None; }
		}
	}
}
#endif