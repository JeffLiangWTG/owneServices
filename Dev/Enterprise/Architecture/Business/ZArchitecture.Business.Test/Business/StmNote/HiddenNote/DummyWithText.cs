using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class DummyWithText : DummyEnterpriseBusinessObject
	{
		public DummyWithText(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString Details
		{
			get { return TextNoteExposed.Text; }
			set { TextNoteExposed.Text = value; }
		}

		public HiddenTextNoteExposed TextNoteExposed
		{
			get
			{
				if (fTextNoteExposed == null)
				{
					fTextNoteExposed = new HiddenTextNoteExposed(this);
				}
				return fTextNoteExposed;
			}
		}

		HiddenTextNoteExposed fTextNoteExposed;
	}
}
