# WorkItemSubscriberDataSchema Changelog
(See [example](../ExampleChangelog.md) for how to update this document)

## [v4] - 2025-03-26
### Removed
- Columns
	- `WKI_Details`. This is rolled back because CDC-enabling this column (on all CW1 installations) could have a massive impact on performance and storage requirement

## [v3] - 2025-02-19 (chrono is out of order because of git rebase)
### Added
- Columns
	- `WKI_Details`

## [v2] - 2025-03-25 (we had to let this get merged first because it's from a regen branch)
### Added
- Columns
	- `WKI_SystemCreateBranch`
	- `WKI_SystemCreateDepartment`

## [v1] - 2024-04-17

### Initial schema

- Columns
	- `WKI_PK`
	- `WKI_WorkItemType`
	- `WKI_WorkItemNumber`
	- `WKI_WorkItemArea`
	- `WKI_Priority`
	- `WKI_ActivitySubtype`
	- `WKI_PortOrCountry`
	- `WKI_Summary`
	- `WKI_ActivityType`
	- `WKI_DateOfChange`
	- `WKI_Risk`
	- `WKI_Status`
	- `WKI_SystemCreateTimeUtc`
	- `WKI_SystemCreateUser`
	- `WKI_SystemLastEditUser`
	- `WKI_P9_DefectFirstMissedInTask`
	- `WKI_GB_AssignedBranch`
	- `WKI_GC_AssignedCompany`
	- `WKI_GE_AssignedDepartment`
	- `WKI_P9_DefectCausedByTask`
	- `WKI_SystemLastEditTimeUtc`
