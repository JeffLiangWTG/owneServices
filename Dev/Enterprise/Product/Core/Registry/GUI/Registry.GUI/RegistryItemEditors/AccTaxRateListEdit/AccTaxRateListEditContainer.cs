using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class AccTaxRateListEditContainer : ZUserControl
	{
		#region Controls

		internal ZArchitecture.ZGrid AccTaxRateGrid;

		#endregion

		public AccTaxRateListEditContainer(RegistryFindBoxFilter filter, BusinessObjectFactory factory, Guid companyPK)
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
				AccTaxRateGrid.ReadOnly = value;
			}
		}

		public string FieldValue
		{
			get { return (Data == null) ? "" : Data.AccTaxRateList.ToString(); }
			set
			{
				if (Data == null)
				{
					Data = new AccTaxRateListCollectionWrapper(value, Filter, Factory, CompanyPK);
					SetDataBinding(Data, null);
				}
				else
				{
					Data.AccTaxRateList.RemoveAll();
					Data.AccTaxRateList.Load(value);
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
		internal AccTaxRateListCollectionWrapper Data;
		bool fReadOnly;
	}
}
