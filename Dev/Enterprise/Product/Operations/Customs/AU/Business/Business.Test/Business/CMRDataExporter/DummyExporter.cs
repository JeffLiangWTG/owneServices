using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class DummyExporter : CMRDataExporterCSV
	{
		public DummyExporter(BusinessObject bizObj)
			: base(bizObj)
		{
		}

		public override string PartFileName
		{
			get { return "ZUBIN"; }
		}

		public override string MailSubject
		{
			get
			{
				return "Contingency Test Report";
			}
		}

		protected override IDataExportCSVFileNameProvider FileNameProvider
		{
			get { return new DummyFileNameProvider(); }
		}

		protected override IDocManagerSupport DocManagerSupporter
		{
			get { return null; }
		}

		public override ZString BodyText
		{
			get
			{
				return "This is the body text";
			}
		}

		public override AdditionalContingencyData AdditionalData
		{
			get
			{
				if (fAdditionalData == null)
				{
					fAdditionalData = new AdditionalContingencyData(Factory);
				}
				return fAdditionalData;
			}
		}
		AdditionalContingencyData fAdditionalData;

		protected override StringCollectionX[] Values
		{
			get
			{
				ArrayList list = new ArrayList();
				StringCollectionX values = new StringCollectionX();
				values.Add(" ");
				values.Add("Hello      ");
				values.Add("Something, with a comma and \"quotes\"");

				list.Add(values);

				return (StringCollectionX[])list.ToArray(typeof(StringCollectionX));
			}
		}
	}
}
