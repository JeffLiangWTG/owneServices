using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.UniversalDataBuss.Matching.Testing
{
	public class OrganisationTestHelper
	{
		public OrganisationTestHelper(UniversalObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, "UniversalObjectFactory factory");
		}
		readonly UniversalObjectFactory factory;

		public IOrgHeader CreateBusinessObject(string firstWord, string secondWord, string fourDigitNumber)
		{
			var organisation = factory.BOFactory.New<IOrgHeader>();
			SetValues(firstWord, secondWord, fourDigitNumber, organisation);
			factory.SaveForTesting(); // Generates the Org Matching Data.
			return organisation;
		}

		public OrganizationAddress CreateDataObject(string firstWord, string secondWord, string fourDigitNumber)
		{
			var organisation = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			SetValues(firstWord, secondWord, fourDigitNumber, new IOrganisationDataWrapper(organisation));
			return organisation;
		}

		void SetValues(string firstWord, string secondWord, string fourDigitNumber, IOrganisationData organisation)
		{
			organisation.FullName = firstWord.ToUpperInvariant() + " " + secondWord.ToUpperInvariant() + " ENTERPRISES";
			organisation.Address1 = fourDigitNumber.Substring(1, 3) + " " + firstWord.ToUpperInvariant() + " BOULEVARDE";
			organisation.City = secondWord.ToUpperInvariant() + "VILLE";
			organisation.Email = firstWord + "@" + secondWord.ToLowerInvariant() + ".com.au";
			organisation.Postcode = fourDigitNumber;

			Tuple<string, string, string> numberGeneratedData = GetDiallingPrefixStateAndUNLOCO(fourDigitNumber);

			organisation.Phone = "0011 61 " + numberGeneratedData.Item1 + " " + fourDigitNumber + " " + fourDigitNumber.Substring(2, 2) + fourDigitNumber.Substring(0, 2);
			organisation.Fax = "0011 61 " + numberGeneratedData.Item1 + " " + fourDigitNumber + " " + fourDigitNumber.Substring(3, 1) + fourDigitNumber.Substring(0, 3);

			organisation.State = numberGeneratedData.Item2;
			organisation.UNLOCO = numberGeneratedData.Item3;
		}

		Tuple<string, string, string> GetDiallingPrefixStateAndUNLOCO(string fourDigitNumber)
		{
			switch (fourDigitNumber.Substring(0, 1))
			{
				case "3":
					return new Tuple<string, string, string>("3", "VIC", "AUMEL");
				case "4":
					return new Tuple<string, string, string>("4", "NSW", "AURCH");
				case "5":
					return new Tuple<string, string, string>("5", "VIC", "AUHTU");
				case "6":
					return new Tuple<string, string, string>("6", "ACT", "AUCBR");
				case "7":
					return new Tuple<string, string, string>("7", "QLD", "AUBNE");
				case "8":
					return new Tuple<string, string, string>("8", "SA", "AUADL");
				case "9":
					return new Tuple<string, string, string>("9", "WA", "AUPER");
				default:
					return new Tuple<string, string, string>("2", "NSW", "AUSYD");
			}
		}

		class IOrganisationDataWrapper : IOrganisationData
		{
			internal IOrganisationDataWrapper(OrganizationAddress target)
			{
				this.target = Argument.NotNull(target, "OrganizationAddress target");
			}
			readonly OrganizationAddress target;

			ZString IOrganisationData.Address1
			{
				get { return target.Address1.GetValueOrDefault(); }
				set { target.Address1 = value; }
			}

			ZString IOrganisationData.Address2
			{
				get { return target.Address2.GetValueOrDefault(); }
				set { target.Address2 = value; }
			}

			ZString IOrganisationData.City
			{
				get { return target.City.GetValueOrDefault(); }
				set { target.City = value; }
			}

			ZString IOrganisationData.Code
			{
				get { return target.OrganizationCode.GetValueOrDefault(); }
				set { target.OrganizationCode = value; }
			}

			ZString IOrganisationData.Email
			{
				get { return target.Email.GetValueOrDefault(); }
				set { target.Email = value; }
			}

			ZString IOrganisationData.Fax
			{
				get { return target.Fax.GetValueOrDefault(); }
				set { target.Fax = value; }
			}

			ZString IOrganisationData.FullName
			{
				get { return target.CompanyName.GetValueOrDefault(); }
				set { target.CompanyName = value; }
			}

			ZString IOrganisationData.Mobile
			{
				get { return target.Mobile.GetValueOrDefault(); }
				set { target.Mobile = value; }
			}

			ZString IOrganisationData.Phone
			{
				get { return target.Phone.GetValueOrDefault(); }
				set { target.Phone = value; }
			}

			ZString IOrganisationData.Postcode
			{
				get { return target.Postcode.GetValueOrDefault(); }
				set { target.Postcode = value; }
			}

			ZString IOrganisationData.State
			{
				get { return ((ZString?)target.State).GetValueOrDefault(); }
				set { target.State = value; }
			}

			ZString IOrganisationData.UNLOCO
			{
				get { return target.Port == null ? ZString.Empty : target.Port.Code.GetValueOrDefault(); }
				set
				{
					target.Port = new UNLOCO() { Code = value };
					target.Country = new Country() { Code = value.Left(2) };
				}
			}

			ZString IOrganisationData.Web
			{
				get { return ""; }
				set { }
			}

			ZGuid IOrganisationData.PK
			{
				get { return ZGuid.Invalid; }
			}
		}
	}
}
