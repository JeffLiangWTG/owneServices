using System;
using System.Collections.Generic;

namespace Enterprise.DocumentEngine
{
	/// <summary>
	/// A documentPrintSet for streaming printing.
	/// WARNING: All calls to List methods (Add, AddRange, Remove, Clear and list indexer) will throw an InvalidOperationException.
	/// </summary>
	public class DocumentPrintSetWithStreaming : DocumentPrintSet
	{
		public DocumentPrintSetWithStreaming(DocumentCommand command, int docPacksCount, IEnumerable<DocumentPack> docPacks, bool supportsLanguageSelection = false)
			: base(command, docPacks)
		{
			this.UseStreamMode = true;
			this.SupportsLanguageSelectionOverride = supportsLanguageSelection;
			this.docPacksCount = docPacksCount;
		}

		readonly int docPacksCount;

		#region Implementation

		public override void ResetCachedReports()
		{
			//Resetting cached reports in base class is done by iterating through the DocPacks list. We don't want to do that in streaming mode.
			//And we don't need it as cached reports get reset when each docpack is disposed after it's been rendered.
			//See Run(PrintTaskSettings TaskSettings) and Run(DeliveryInstructions deliveryInstructions).
		}

		protected override void Dispose(bool disposing)
		{
			//Disposal of DocumentPacks in base class is done by iterating through the DocPacks list. We don't want to do that in streaming mode.
			//Instead, each DocPack is disposed once rendered. See Run(PrintTaskSettings TaskSettings) and Run(DeliveryInstructions deliveryInstructions).
		}

		public override DocumentPack this[int index]
		{
			get
			{
				throw new InvalidOperationException("DocumentPacks is not implemented as a List. It cannot be accessed through this indexer.");
			}
			set
			{
				throw new InvalidOperationException("DocumentPacks is not implemented as a List. It cannot be accessed through this indexer.");
			}
		}

		#region Wrapped List Methods and Properties

		public override void Add(DocumentPack item)
		{
			throw new InvalidOperationException("DocumentPacks is not implemented as a List. Cannot use Add mothod.");
		}

		public override void AddRange(IEnumerable<DocumentPack> collection)
		{
			throw new InvalidOperationException("DocumentPacks is not implemented as a List. Cannot use AddRange mothod.");
		}

		public override void Remove(DocumentPack item)
		{
			throw new InvalidOperationException("DocumentPacks is not implemented as a List. Cannot use Remove mothod.");
		}

		public override void Clear()
		{
			throw new InvalidOperationException("DocumentPacks is not implemented as a List. Cannot use Clear mothod.");
		}

		public override int Count
		{
			get { return docPacksCount; }
		}

		#endregion

		#endregion
	}
}
