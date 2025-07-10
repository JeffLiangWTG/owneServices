using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class ReadOnlyAdditionalInfoCollection : NonPersistentBusinessObjectCollection<ReadOnlyAdditionalInfo>
	{
		public ReadOnlyAdditionalInfoCollection(CusEntryLine entryLine)
			: base(entryLine.Factory)
		{
			EntryLine = Argument.NotNull(entryLine, nameof(entryLine));
		}

		CusEntryLine EntryLine { get; }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new InvalidOperationException("It is not possible to create a new grouped previous document object from this collection");
		}

		protected override bool AllowNewCore => false;

		public void LoadNew()
		{
			RemoveAll();

			AddEntryLineAdditionalInfos();
			AddNewOrModifiedMergedAdditionalInfos();

			base.IsLoaded = true;
		}

		void AddEntryLineAdditionalInfos()
		{
			var entryLineAdditionalInfos = EntryLine.GetPreviouslySentAdditionalInfos();
			entryLineAdditionalInfos.ForEach(entryLineDoc => Add(new ReadOnlyAdditionalInfo(entryLineDoc)));
		}

		void AddNewOrModifiedMergedAdditionalInfos()
		{
			var mergedDocumentCollection = new ReadOnlyAdditionalInfoCollection(EntryLine);
			mergedDocumentCollection.InitializeWithMergedAdditionalInfos();
			var modifiedOrNewDocuments = mergedDocumentCollection.Cast<ReadOnlyAdditionalInfo>().Except(this.Cast<ReadOnlyAdditionalInfo>(), new AcceptedDocumentModificationComparer()).Cast<ReadOnlyAdditionalInfo>();
			modifiedOrNewDocuments.ForEach(modifiedOrNewDoc => Add(modifiedOrNewDoc));
		}

		void InitializeWithMergedAdditionalInfos()
		{
			EntryLine.Declaration.AdditionalInfos.Cast<AdditionalInfo>().ForEach(addInf => AddAdditionalInfo(addInf));
			EntryLine.Header.EntryInstruction?.AdditionalInfos.Cast<AdditionalInfo>().ForEach(addInf => AddAdditionalInfo(addInf));
			EntryLine.Header.InvoiceHeaders.ForEach(header => ((JobComInvoiceHeader)header).AdditionalInfos.Cast<AdditionalInfo>().ForEach(addInf => AddAdditionalInfo(addInf)));
			EntryLine.InvoiceLines.ForEach(line => ((JobComInvoiceLine)line).AdditionalInfos.Cast<AdditionalInfo>().ForEach(addInf => AddAdditionalInfo(addInf)));
		}

		void AddAdditionalInfo(AdditionalInfo additionalInfo)
		{
			Add(new ReadOnlyAdditionalInfo(additionalInfo));
		}

		#region AcceptedDocumentModificationComparer

		public class AcceptedDocumentModificationComparer : IEqualityComparer<IAdditionalInfoEqualityKey>
		{
			public bool Equals(IAdditionalInfoEqualityKey x, IAdditionalInfoEqualityKey y)
			{
				return x.CSI_Code == y.CSI_Code
					&& x.CSI_Description == y.CSI_Description
					&& x.CSI_SubType == y.CSI_SubType
					&& x.CSI_ReferenceNumber == y.CSI_ReferenceNumber
					&& x.CSI_ReferenceNumber2 == y.CSI_ReferenceNumber2
					&& x.CSI_RX_NKCurrency == y.CSI_RX_NKCurrency
					&& x.CSI_Value == y.CSI_Value;
			}

			public int GetHashCode(IAdditionalInfoEqualityKey obj) => FixedHashCodeOnlyForObjectEqualityComparison;
			const int FixedHashCodeOnlyForObjectEqualityComparison = 1;
		}

		#endregion
	}
}
