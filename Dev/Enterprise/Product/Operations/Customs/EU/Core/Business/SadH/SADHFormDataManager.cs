using System;
using Enterprise.Customs.EU.Business.Declaration;
using ISADHFormDataManager = Enterprise.Customs.Business.SADH.ISADHFormDataManager;

namespace Enterprise.Customs.EU.Business.SADH
{
	public class SADHFormDataManager : ISADHFormDataManager
	{
		public SADHFormDataManager(JobDeclaration declaration)
		{
			if (declaration == null)
			{
				throw new ArgumentNullException(nameof(declaration));
			}
			this.declaration = declaration;
			FormData = new SADHFormData(declaration.Factory, declaration);
			new SADHFormDataReader(FormData).ReadFrom(declaration);
		}
		readonly JobDeclaration declaration;
		public readonly SADHFormData FormData;

		public void WriteData()
		{
			new JobDeclarationWriter(declaration).WriteFrom(FormData);
			ExecutedSuccessfully = true;
		}

		Customs.Business.SADH.SADHFormData ISADHFormDataManager.FormData
		{
			get { return FormData; }
		}

		public bool ExecutedSuccessfully { get; set; }
	}
}
