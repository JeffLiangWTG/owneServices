using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class OrgHeaderCodeListEditContainer : ZUserControl
	{
		public OrgHeaderCodeListEditContainer()
		{
			InitializeComponent();
		}

		public Guid[] FieldValue
		{
			get
			{
				return (Data == null) ? Array.Empty<Guid>() : Data.OrgHeaderCodeList.ToGuidArray();
			}
			set
			{
				if (Data == null)
				{
					Data = new OrgHeaderCodeListCollectionWrapper(value);
					SetDataBinding(Data, "");
				}
				else
				{
					Data.OrgHeaderCodeList.RemoveAll();
					Data.OrgHeaderCodeList.Load(value);
				}
			}
		}

		OrgHeaderCodeListCollectionWrapper Data;

		#region ReadOnly

		public bool ReadOnly
		{
			get { return fReadOnly; }
			set
			{
				fReadOnly = value;
				OrgHeaderCodeGrid.ReadOnly = value;
			}
		}

		bool fReadOnly;

		#endregion
	}
}
