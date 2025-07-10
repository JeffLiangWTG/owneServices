using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IShowEditOrViewForm
	{
		ZFilterModule NewModuleFromModuleID();
		ShowingEditOrViewFormEventArgs InitialiseForm(ZFilterModule module);
		string SearchCode { get; }
		bool ShowNewFormWhenEmpty { get; }

		bool AllowNewForm { get; }
		bool ReadOnly { get; set; }

		object DataSource { get; }

		IEnumerable<BusinessObject> GetBizObjsToEditOrView();
		void ShowMoreThanOneSelectedMessageAndPopup(bool isEdit);

		DialogResult ShowDefaultMessageWhenCreatingANewBizObjFromFindBox(ZFilterModule module);

		void SetCodePropertyFromText(IZForm form);
	}
}
