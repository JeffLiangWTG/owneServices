using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Interop.DataObjects;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.PlugIn.PlugInTesting;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.PlugIn
{
	sealed class eDocsPlugInForTesting : eDocsPlugIn
	{
		public eDocsPlugInForTesting(IBusiness host)
			: base(host)
		{
		}

		internal new StorageMain TopLevelParentMain => base.TopLevelParentMain;
		internal new bool hadExceptionGettingTopLevelParentMain => base.hadExceptionGettingTopLevelParentMain;
		internal StorageMain TopLevelParentMainForTesting => TopLevelParentMain;
		internal StorageMain ResetTopLevelParentMain(DocumentFactory factory)
		{
			base.masterFactory = factory;
			base.topLevelParentMain = null;
			return base.TopLevelParentMain;
		}

		internal new string NotDisplayedMessage => base.NotDisplayedMessage;
		internal new bool IsDropDataSupported(DragEventArgs e) => base.IsDropDataSupported(e);
		internal new Dictionary<string, StorageDocsBase> ExtractEmbeddedRtfImagesAndAddToEDocs(IEmbeddedRtfImageSource embeddedRtfImageSource)
			=> base.ExtractEmbeddedRtfImagesAndAddToEDocs(embeddedRtfImageSource);

		internal new ZForm parentFormOfPlugin => base.parentFormOfPlugin;

		internal new Dictionary<string, StorageDocsBase> InsertFromData(IDataObject data) => base.InsertFromData(data);

		internal new DragDropEffects GetDragDropEffect(DragEventArgs e) => base.GetDragDropEffect(e);

		internal new void AddRequiredDocument(StorageDocsBase storedDocument) => base.AddRequiredDocument(storedDocument);

		protected override FileAction ShowOverwriteOrCreateNewPrompt(string filename)
		{
			if (UseCreateNew)
			{
				return FileAction.CreateNew;
			}

			if (UseOverwrite)
			{
				return FileAction.Overwrite;
			}

			return FileAction.None;
		}

		protected override ZDataObject GetSingleImageDropDataObject(string imageFile)
		{
			if (Enterprise.ZArchitecture.Environment.Globals.IsTest)
			{
				return new MockDataObject(new DataObject(DataFormats.FileDrop, new string[] { imageFile }));
			}

			return base.GetSingleImageDropDataObject(imageFile);
		}

		public Func<IDataObject, ZDataObject> GetZDataObjectFromDataForTest { get; set; }

		protected override ZDataObject GetZDataObjectFromData(IDataObject dataToInsert)
		{
			return GetZDataObjectFromDataForTest != null ? GetZDataObjectFromDataForTest(dataToInsert) : base.GetZDataObjectFromData(dataToInsert);
		}

		protected override Control GetNewUserControl()
		{
			IsUserControlExist = true;
			return base.GetNewUserControl();
		}

		public bool UseCreateNew { get; set; }
		public bool UseOverwrite { get; set; }
		public ZBool AllowPlugInDisplayWithNoLicenceExposed => AllowPlugInDisplayWithNoLicence;
		public void OnParentFormDataObjectPastedDone(ZForm form, DataObjectPastedEventArgs e) => OnParentFormDataObjectPasted(form, e);
		public bool ShouldPlugInGUIAndBusinessEntityBeCreatedCoreForTest() => ShouldPlugInGUIAndBusinessEntityBeCreatedCore();
		public void OnParentFormDragDropDone(ZForm form, DragEventArgs e) => OnParentFormDragDrop(form, e);
		public void OnParentFormDragOverDone(ZForm form, DragEventArgs e) => OnParentFormDragOver(form, e);
		public void SetupForTesting() => Setup();
		public void SetupUserControlForTesting() => SetupUserControl();
		public void SynchroniseIfTabPageVisibleForTesting() => SynchroniseIfTabPageVisible();
		public bool IsSecurityGrantedForTesting => IsSecurityGranted;
		public bool IsSecurityGrantedEditForTesting => IsSecurityGrantedEdit;
		public bool IsUserControlVisibleForTest => IsUserControlVisible;
		public bool IsUserControlExist { get; private set; }
	}
}
