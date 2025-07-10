using System;
using System.IO;
using System.Threading.Tasks;

namespace CargoWise.BuildTools
{
	public interface ISubmissionsApiClient
	{
		void CreateNewSubmission(SubmissionType submissionType, string userName, string taskComments, Guid processTaskPK, string criticality = "");
		Guid GetLatestBuild(string repositoryUrl, int pullRequestId);
		Task<Stream> DownloadLatestTestMethodsFileAsync(string repositoryUrl, string branch = "master", string path = "/", string buildConfiguration = "DEBUG");
		Task<string> UploadTestFailureDataAndGetUrlAsync(Stream stream, string contentType = "text/plain", string fileExtension = ".txt");
	}
}
