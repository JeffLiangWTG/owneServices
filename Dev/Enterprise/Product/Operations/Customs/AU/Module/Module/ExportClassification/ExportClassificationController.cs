using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Module
{
	/// <summary>
	/// Module Controller for ExportClassification.
	/// </summary>
	public class ExportClassificationController : Customs.Module.ExportClassificationController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(Classification); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ExportClassificationForm((Classification)businessEntity);
		}
	}
}
