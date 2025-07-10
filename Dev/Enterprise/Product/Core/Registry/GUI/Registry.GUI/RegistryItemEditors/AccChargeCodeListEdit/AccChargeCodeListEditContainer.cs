using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class AccChargeCodeListEditContainer : ZUserControl
	{
		public AccChargeCodeListEditContainer(RegistryFindBoxFilter filter, BusinessObjectFactory factory, Guid companyPK)
		{
			this.Filter = filter;
			this.Factory = factory;
			this.CompanyPK = companyPK;
			InitializeComponent();
		}

		public bool ReadOnly
		{
			get { return fReadOnly; }
			set
			{
				fReadOnly = value;
				AccChargeCodeGrid.ReadOnly = value;
			}
		}

		public string FieldValue
		{
			get { return (Data == null) ? "" : Data.AccChargeCodeList.ToString(); }
			set
			{
				if (Data == null)
				{
					Data = new AccChargeCodeListCollectionWrapper(value, Filter, Factory, CompanyPK);
					SetDataBinding(Data, null);
				}
				else
				{
					Data.AccChargeCodeList.RemoveAll();
					Data.AccChargeCodeList.Load(value);
				}
			}
		}
		protected override void OnValidating(System.ComponentModel.CancelEventArgs e)
		{
			base.OnValidating(e);
			Data.RunPreSaveValidation();
			if (Data.HasErrors)
			{
				e.Cancel = true;
				using (var form = new ZErrorMessageBox(Data))
				{
					ZFormModaliser.ShowMessageBoxWithoutDispose(form);
				}
			}
		}

		internal readonly BusinessObjectFactory Factory;
		internal readonly Guid CompanyPK;
		internal readonly RegistryFindBoxFilter Filter;
		internal AccChargeCodeListCollectionWrapper Data;
		bool fReadOnly;
	}
}
