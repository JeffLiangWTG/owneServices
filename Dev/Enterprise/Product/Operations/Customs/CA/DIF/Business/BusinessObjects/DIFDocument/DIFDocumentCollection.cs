using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.DIF.Business
{
	public class DIFDocumentCollection : DISDocumentCollectionBase<DIFDocument>
	{
		public DIFDocumentCollection(DIFHostWrapper wrapper)
			: base(wrapper)
		{
		}

		new DIFHostWrapper wrapper => base.wrapper as DIFHostWrapper;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var result = new DIFDocument(wrapper)
			{
				BusinessNumber =
					wrapper.DISHost.BusinessNumberHolder?.CustomsCodes.Cast<OrgCusCode>()
						.FirstOrDefault(code => BusinessNumberCodeTypesForDIF.GetDIFBusinessNumberCodeTypes().Contains(code.OK_CodeType))
						?.OK_CustomsRegNo ?? ZString.Empty
			};

			return result;
		}

		protected override string XmlNamespace => DIFDocument.Constants.XmlNamespace;

		protected override string ApplciationCode => Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;

		protected override IDISDocumentBase CreateElement() => new DIFDocument(wrapper);
	}
}
