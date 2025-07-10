using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public class AHECCTariffGridFindBox : ZGridFindBox
	{
		public AHECCTariffGridFindBox(IsImportDelegate isImportDelegate)
		{
			this.isImportDelegate = isImportDelegate;
		}
		readonly IsImportDelegate isImportDelegate;

		protected internal bool IsImport => isImportDelegate != null && isImportDelegate();

		protected override IFindBoxPopup GetNewPopupForm()
		{
			return BorderWiseTariffFindBoxProvider.GetTariffFindBoxWrapper(IsImport ? "I" : "E", GetNewTariffPopupForm);
		}

		IFindBoxPopup GetNewTariffPopupForm()
		{
			if (IsImport)
			{
				return new AUCClassForm();
			}
			else
			{
				return new AHECCForm();
			}
		}

		protected override IFindBoxListProvider ListProvider
		{
			get
			{
				if (IsImport)
				{
					var bizo = (FindForm() as ZForm)?.BusinessEntity;
					var factory = bizo?.Factory ?? new BusinessObjectFactory();
					return new AUCClassFindBoxListProvider(factory);
				}
				else
				{
					return new AHECCFindBoxListProvider();
				}
			}
		}
	}

	public delegate bool IsImportDelegate();
}
