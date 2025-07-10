using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.OperationalActions
{
	public class FrCreditCODApplicator : OperationalActionMethodApplicator, IOverrideSelectionCount
	{
		public FrCreditCODApplicator(BusinessObjectFactory factory) : base((NoResString)"Credit COD", factory)
		{
		}

		protected override void BuildCore(ZGuid[] selectItemPKs)
		{
			var items = FrCreditCODItemApplicators;
			foreach (var selectItemPK in selectItemPKs)
			{
				var declaration = Factory.Load<JobDeclaration>(selectItemPK);
				if (declaration != null)
				{
					var entries = declaration.CustomsEntryHeaders.Where(x => !x.CH_BGMReference.IsEmpty);
					foreach (var entryHeader in entries)
					{
						var item = items.AddNew();
						item.ReleasingEntryReference = entryHeader.CH_BGMReference.Left(item.ReleasingEntryReferenceInfo.MaxLength);
					}
				}
				else
				{
					var entryHeader = Factory.Load<CusEntryHeader>(selectItemPK);
					if (entryHeader != null && !entryHeader.CH_BGMReference.IsEmpty)
					{
						var item = items.AddNew();
						item.ReleasingEntryReference = entryHeader.CH_BGMReference.Left(item.ReleasingEntryReferenceInfo.MaxLength);
					}
				}
			}
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var runner = new CreditCODOperationalActionRunner(log);
			runner.CreditCOD(FrCreditCODItemApplicators);
		}

		[ChildEditable]
		public CreditCODDataObjectCollection FrCreditCODItemApplicators
		{
			get
			{
				if (frCreditCODItemApplicators == null)
				{
					frCreditCODItemApplicators = new CreditCODDataObjectCollection(this);
					RegisterEditableChildObject(frCreditCODItemApplicators);
				}

				return frCreditCODItemApplicators;
			}
		}
		CreditCODDataObjectCollection frCreditCODItemApplicators;

		public int SelectionCount => FrCreditCODItemApplicators.Count;

		public bool SupportRunningOnAllMatchedRecords => false;
	}
}
