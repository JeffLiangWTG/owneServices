using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class Import5ULHeaderWrapper : NonPersistentBusinessObject,
		IVisualizerNoteSupporter
	{
		public Import5ULHeaderWrapper(ZGuid pk, Import5ULHeader header, BusinessObjectFactory factory)
			: base(factory)
		{
			Header = header;
			EntryPK = pk;
		}

		#region IVisualizerNoteSupporter members
		ZGuid IVisualizerNoteSupporter.PK => EntryPK;
		ZGuid IVisualizerNoteSupporter.ChildBusinessObjectPK => ZGuid.Empty;
		string IVisualizerNoteSupporter.TableCode => CusEntryHeaderSchema.Constants.Prefix;
		#endregion

		public IImport5ULHeader Header { get; }
		ZGuid EntryPK { get; }

		public Import5ULEntryLineWrapperCollection EntryLineItems
		{
			get
			{
				if (entryLineItems == null)
				{
					entryLineItems = new Import5ULEntryLineWrapperCollection((Import5ULHeader)Header, Factory);
				}
				return entryLineItems;
			}
		}
		Import5ULEntryLineWrapperCollection entryLineItems;

		public OrganizationDocWrapper Payer => payer ?? (payer = new OrganizationDocWrapper(Header.Payer));
		OrganizationDocWrapper payer;

		public ZString FormattedRefundDeclarationNumber => MessageFunctions.DeclarationNumberFormat(Header.RefundDeclarationNumber);

		public ZString CustomsOfficeName => MessageFunctions.GetCustomsOffice(Factory, Header.DeclarationCustomsOffice);

		public OrganizationDocWrapper Declarant { get; set; }
		public ZString EntryStatus { get; set; }
		public ZDateTime EntryReleaseDate { get; set; }
		public ZDateTime IssueDateTo5UL { get; set; }
		public ZString BankName
		{
			get
			{
				return Factory.GetCachedValue<BankTypeList>().GetDescriptionFromCode(Header.BankCode);
			}
		}

		public ZString RefundCauseCodeDescription
		{
			get
			{
				return Factory.GetCachedValue<RefundCauseCodeList>().GetDescriptionFromCode(Header.RefundCauseCode);
			}
		}
	}
}
