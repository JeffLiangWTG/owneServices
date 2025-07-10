using System;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.JobApplication
{
	[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Not (yet) needed.")]
	public struct JobApplicationCreationRequestData
	{
		public Guid campaign_pk { get; set; }

		public string email_address { get; set; }

		public string full_name { get; set; }

		public string mobile_phone { get; set; }

		public string country_code { get; set; }

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Needs to interface with JS")]
		public JobApplicationCreationDocument[] documents { get; set; }

		public string referring_source {  get; set; }

		public string address1 { get; set; }

		public string address2 { get; set; }

		public string city { get; set; }

		public string state_code { get; set; }

		public DateTime date_of_birth { get; set; }

		public string nationality_iso_code { get; set; }

		public string gender { get; set; }

		public string work_permit_status { get; set; }

		public string availability { get; set; }
	}

	[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Not (yet) needed.")]
	public struct JobApplicationCreationDocument
	{
		public string file_name { get; set; }

		public string data_type { get; set; }

		public string document_type { get; set; }

		public string document_content { get; set; }

		public string document_content_sha256_hash { get; set; }
	}
}
