using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class OrgDebtorGroupCodeListEditContainer : ZUserControl
	{
		public OrgDebtorGroupCodeListEditContainer()
		{
			InitializeComponent();
		}

		public Guid[] FieldValue
		{
			get
			{
				return (Data == null) ? Array.Empty<Guid>() : Data.OrgDebtorGroupCodeList.ToGuidArray();
			}
			set
			{
				if (Data == null)
				{
					Data = new OrgDebtorGroupCodeListCollectionWrapper(value);
					SetDataBinding(Data, "");
				}
				else
				{
					Data.OrgDebtorGroupCodeList.RemoveAll();
					Data.OrgDebtorGroupCodeList.Load(value);
				}
			}
		}

		OrgDebtorGroupCodeListCollectionWrapper Data;

		#region ReadOnly

		public bool ReadOnly
		{
			get { return fReadOnly; }
			set
			{
				fReadOnly = value;
				OrgDebtorGroupCodeGrid.ReadOnly = value;
			}
		}

		bool fReadOnly;

		#endregion				
	}
}
