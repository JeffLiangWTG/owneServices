using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CustomsWare.Services;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.CustomsWare.Business
{
	public class CustomsWareIntegration : CusIntegrationProvider
	{
		protected override XElement Submit(ZString submittedData)
		{
			return new CustomsWareServices().ExecApiSafe(Settings.Instance, submittedData);
		}

		protected override ZString GetDataToSubmit()
		{
			var dataAdapter = InputDocumentValueObjectDataAdapter.New();
			var inputDocument = dataAdapter.ExportToValueObject(declaration,
				new ValueObjectExportContext(new NotificationBuffer()));

			var textWriter = new StringWriter();
			ZXmlSerializer.New(typeof(XSD.InputDocument)).Serialize(textWriter, inputDocument);

			return textWriter.ToString();
		}

		protected override bool SubmitSucceeded(XElement submissionResult)
		{
			var status = submissionResult.Descendants("StatusCode").FirstOrDefault();
			return status != null && status.Value == "0";
		}

		protected override ZString GetSubmitFailedReasons(XElement submissionResult)
		{
			var result = ZString.Empty;
			IEnumerable<XElement> errors =
				from error in submissionResult.DescendantsAndSelf("ErrorItem")
				select error;

			var status = submissionResult.Descendants("StatusCode").FirstOrDefault();
			if (status != null)
			{
				result = Res.GetString("8df65f0e-c727-4585-b80d-5af627a4c7cb", "Status Code: {0}", status.Value) + "\n";
			}

			var errorList = new ZStringBuilder();
			foreach (var error in errors)
			{
				errorList.Append("\n");

				var element = error.Descendants("ErrorIdentifier").FirstOrDefault();
				if (element != null)
				{
					errorList.Append(Res.GetString("c59cc6a6-6e71-49fc-b6a6-5636d8e72428", "Error Identifier:") + " ");
					errorList.Append(element.Value);
					errorList.Append("\n");
				}

				element = error.Descendants("ErrorCode").FirstOrDefault();
				if (element != null)
				{
					errorList.Append(Res.GetString("def408ec-04fe-45c5-86a8-a278ec52a3e1", "Error Code:") + " ");
					errorList.Append(element.Value);
					errorList.Append("\n");
				}

				element = error.Descendants("ErrorDescription").FirstOrDefault();
				if (element != null)
				{
					errorList.Append(Res.GetString("13c27e18-a954-4919-aa33-ac242a60e899", "Error Description:") + " ");
					errorList.Append(element.Value);
					errorList.Append("\n");
				}
			}

			result += errorList.ToString();

			if (result.IsEmpty)
			{
				result = submissionResult.ToString();
			}

			return result;
		}

		protected override ZString ApplicationCode
		{
			get { return ApplicationCodeList.Codes.CustomsWare; }
		}

		protected override ICollection<SettingDetail> SettingsToValidate
		{
			get
			{
				var result = base.SettingsToValidate;
				result.Add(new SettingDetail(Settings.Instance.Uri, "Uri"));
				result.Add(new SettingDetail(Settings.Instance.Company, "Company"));
				return result;
			}
		}

		protected override ZString ProviderName
		{
			get { return Res.GetString("D5AE5EB8-2154-4EE1-B728-CA937FB44FA0", "Web Service"); }
		}

		protected override ZString RegistryLocation
		{
			get { return Res.GetString("AE9C833F-6013-454A-89E1-599F37D098A3", "System > Registry > Customs > Integration > ABM"); }
		}
	}
}
