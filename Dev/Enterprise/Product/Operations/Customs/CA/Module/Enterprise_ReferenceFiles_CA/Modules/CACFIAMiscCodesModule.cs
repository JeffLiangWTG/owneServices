using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Universal.Module;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	/// <summary>
	/// Module for CACFIAMiscCodes.
	/// </summary>
	[SuppressFormsLocalizedTest]
	public class CACFIAMiscCodesModule : ZZRefCusCodeListWrapperModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.CA.CFIAMiscCodes; }
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CACFIAMiscCodesCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ZZRefCusCodeListWrapperFilterStripBusinessObject();
		}

		#region Captions

		protected override string CodeCaption => Res.GetString("6B4AEE08-3EDC-433f-83CD-731B3772F1FF", "Miscellaneous Code");
		protected override string DescriptionCaption => Res.GetString("B0AF1739-8210-4A23-9776-A96D088B31AB", "Description");
		protected override int CodeCaptionColumnWidth => 200;
		protected override int DescriptionColumnWidth => 500;

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion
	}
}
