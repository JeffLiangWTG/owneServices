using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class GlobalDrawbackCusEntryLineCollection : GlobalCusEntryLineCollection
	{
		public GlobalDrawbackCusEntryLineCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public GlobalDrawbackCusEntryLineCollection(BusinessObjectFactory factory, BaseJobDeclaration jobDec) : base(factory, jobDec)
		{
		}

		public GlobalDrawbackCusEntryLineCollection(BusinessObjectFactory factory, BaseJobComInvoiceLine invoiceLine) : base(factory, invoiceLine)
		{
			DefaultDrawbackModuleFilter();
		}

		public void DefaultDrawbackModuleFilter()
		{
			base.DefaultModuleFilterFields();
			if (!ExportDrawbackDate.IsEmpty)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Drawback Date", "PropertySearch", new ZString("Date Range"), false));
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Drawback Date", "Property1", ZDateTime.Empty, false));
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Drawback Date", "Property2", ExportDrawbackDate, false));
			}
		}

		public ZDateTime ExportDrawbackDate
		{
			get
			{
				var drawbackDate = ZDateTime.Empty;
				if (fJobDec != null && fJobDec.JE_MessageType == JobMessageTypeList.Codes.Drawback)
				{
					foreach (BaseJobComInvoiceHeader invHeader in fJobDec.Invoices)
					{
						if (drawbackDate.IsEmpty || invHeader.JZ_InvoiceDate < drawbackDate)
						{
							drawbackDate = invHeader.JZ_InvoiceDate;
						}
					}
				}

				return drawbackDate;
			}
		}

		#region FindBox List Provider

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new GlobalDrawbackCusEntryLineListProvider(this); }
		}

		public class GlobalDrawbackCusEntryLineListProvider : FindBoxListProvider
		{
			public GlobalDrawbackCusEntryLineListProvider(GlobalDrawbackCusEntryLineCollection collection)
				: base(collection)
			{
			}

			protected override IEnumerable<BusinessObject> BizObjsFromCodeWithCompleteFilter(string code)
			{
				return Enumerable.Empty<BusinessObject>();
			}

			protected override IEnumerable<BusinessObject> BizObjsFromCodeWithRelationshipFilter(string code)
			{
				return Enumerable.Empty<BusinessObject>();
			}

			public override (string, bool) NearestMatchCore(string code, bool explicitAutoComplete)
			{
				return (code, false);
			}
		}

		#endregion
	}
}
