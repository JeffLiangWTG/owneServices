using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IdentityCertificate.Business
{
	public class EdiIdentityCertificate : AutoEdiIdentityCertificate
	{
		public EdiIdentityCertificate(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public LicenceDatabase LicenseDatabase => Factory.Load<LicenceDatabase>(Application.IDA_LD);

		public EdiIdentityApplication Application => Factory.Load<EdiIdentityApplication>(ICE_IDA);

		[RelatedBusinessObject("Application")]
		public override ZGuid ICE_IDA
		{
			get => base.ICE_IDA;
			set => base.ICE_IDA = value;
		}

		[ResourceStringData("548C2860-088E-41D9-935C-65142832A298", Caption = "CSR File Location")]
		public ZString CSRFileLocation { get; set; }

		public bool CSRFileLocation_ReadOnly => true;

		public bool ICE_CARoot_ReadOnly => IsInDatabase;

		[List("CARootLists")]
		[ResourceStringData("49D165F5-C7FA-4ED3-927B-DAA4AB238C38", Caption = "AWS Issuing CA")]
		public override ZString ICE_CARoot { get => base.ICE_CARoot; set => base.ICE_CARoot = value; }

		public string Arn => CARootLists.ContainsCode(ICE_CARoot) ? CARootLists[ICE_CARoot].Description : string.Empty;

		public CodeDescriptionPairList CARootLists
		{
			get
			{
				var rootList = new CodeDescriptionPairList();
				foreach (AWSPrivateCA item in EDIDataRegistry.Instance.AWSPrivateCAListManager.Value)
				{
					if (item.IsEnabled)
					{
						rootList.AddPair(item.IssuingCA, item.Arn);
					}
				}
				return rootList;
			}
		}

		public override void OnSaving()
		{
			ICE_SequenceNumber = Env.NumberFountains.GetEdiIdentityCertificateSequenceNumber().GetNext(Factory);

			base.OnSaving();
		}

		#region Test Data
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			ICE_CertificateSigningRequest = Guid.NewGuid().ToString();
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			var application = Factory.New<EdiIdentityApplication>();
			application.IDA_LD = licenceDatabase.PK;
			ICE_IDA = application.PK;
			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif
		#endregion
	}
}
