using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.ProductCatalog.Outgoing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business.ProductCatalog
{
	public class ProductCatalogProvider : IProduct
	{
		public ProductCatalogProvider(GoodsCatalogMessageSendingObject sendingObject)
		{
			this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
			goodsCatalog = Argument.NotNull(sendingObject.GoodsCatalog, nameof(sendingObject.GoodsCatalog));
		}
		readonly CusGoodsCatalog goodsCatalog;
		readonly GoodsCatalogMessageSendingObject sendingObject;

		public int Sequence => 1;

		public long? AuthorityIdentifier => long.TryParse(goodsCatalog.CGC_AuthorityIdentifier, out var id) ? id : null;

		public string Description => goodsCatalog.ComplementaryDescription;

		public string Denomination => goodsCatalog.CGC_Description;

		public string ConsigneeRegistrationNumber => goodsCatalog.Owner?.GetRootCNPJFromCNPJ() ?? ZString.Empty;

		public string Situation
		{
			get
			{
				switch (sendingObject.Action)
				{
					case ActionList.Codes.Activate:
					case ActionList.Codes.CreateNewVersion:
						return Constants.Situation.Active;
					case ActionList.Codes.Deactivate:
						return Constants.Situation.Deactive;
					default:
						return Constants.Situation.Draft;
				}
			}
		}

		public string Type => GoodsCatalogTypeList.MapToCustomsCode(goodsCatalog.CGC_Type);

		public string Tariff => goodsCatalog.CGC_Tariff;

		public string Version => null;

		public string ReferenceData => null;

		public IEnumerable<IAttribute> Attributes => fAttributes ??= goodsCatalog.Attributes.GetEffectiveAttributes(w => !w.IsCompoundAttribute && !w.ParentIsCompoundAttribute && !w.AllowMultipleAnswersForString).Select(AttributeProvider.New).ToArray();
		IAttribute[] fAttributes;

		public IEnumerable<ICompositeAttribute> CompositeAttributes => fCompositeAttributes ??= goodsCatalog.Attributes.GetEffectiveAttributes(w => w.IsCompoundAttribute).Select(CompositeAttributeProvider.New).WhereNotNull().ToArray();
		ICompositeAttribute[] fCompositeAttributes;

		public IEnumerable<string> LocalPartNumbers => fLocalPartNumbers ??= goodsCatalog.LocalPartNumbers.Where(x => !x.CGI_Reference.IsEmpty).Select(x => x.CGI_Reference.ToString()).ToArray();
		IEnumerable<string> fLocalPartNumbers;

		public IEnumerable<IMultivaluedAttribute> MultivaluedAttributes => fMultivaluedAttributes ??= goodsCatalog.Attributes.GetEffectiveAttributes(w => w.AllowMultipleAnswersForString).Select(MultivaluedAttributeProvider.New).WhereNotNull().ToArray();
		IMultivaluedAttribute[] fMultivaluedAttributes;
	}
}
