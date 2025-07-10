using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocAuthorityToDeal : DocBaseWrapper
	{
		DocAuthorityToDeal(AuthorityToDeal authorityToDeal, BusinessObjectFactory factory)
			: base(authorityToDeal, factory)
		{
		}

		public static DocAuthorityToDeal New(AuthorityToDeal authorityToDeal, BusinessObjectFactory factory)
		{
			return (authorityToDeal == null) ? null : new DocAuthorityToDeal(authorityToDeal, factory);
		}

		AuthorityToDeal AuthorityToDeal
		{
			get { return (AuthorityToDeal)WrappedObject; }
		}

		public override string ToString()
		{
			return SecurityCode;
		}

		public ZString SecurityCode
		{
			get { return AuthorityToDeal.SecurityCode; }
		}

		public ZString PaymentFinalisedDate
		{
			get { return AuthorityToDeal.PaymentFinalisedDateString; }
		}

		public ZString AuthorityToDealDateIssued
		{
			get { return AuthorityToDeal.AuthorityToDealDateIssued; }
		}

		public ZString VersionNumber
		{
			get { return AuthorityToDeal.VersionNumber; }
		}

		public ZString TotalNumberOfPacakges
		{
			get { return AuthorityToDeal.TotalNumberOfPacakges; }
		}

		public ZString WarehouseEstablishmentIdentifier
		{
			get { return AuthorityToDeal.WarehouseEstablishmentIdentifier; }
		}

		public DocAuthorityToDealLineConditionDetailsCollection MessageLines
		{
			get
			{
				if (fMessageLines == null)
				{
					fMessageLines = new DocAuthorityToDealLineConditionDetailsCollection(Factory);
					foreach (AuthorityToDealLineConditionDetails line in AuthorityToDeal.Lines)
					{
						fMessageLines.Add(DocAuthorityToDealLineConditionDetails.New(line, Factory));
					}
				}

				return fMessageLines;
			}
		}

		protected DocAuthorityToDealLineConditionDetailsCollection fMessageLines;
	}
}
