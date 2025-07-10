# WorkProjectSubscriberDataSchema Changelog
(See [example](../ExampleChangelog.md) for how to update this document)

## [v4] - 2025-03-26
### Removed
- Columns
	- `WKP_Details`. This is rolled back because CDC-enabling this column (on all CW1 installations) could have a massive impact on performance and storage requirement

## [v3] - 2025-02-19 (chrono is out of order because of git rebase)
### Added
- Columns
	- `WKP_Details`

## [v2] - 2025-03-25 (we had to let this one get merged first because it's from a regen branch)
### Added
- Columns
	- `WKP_SystemCreateBranch`
	- `WKP_SystemCreateDepartment`

## [v1] - 2024-04-17

### Initial schema

- Columns
	- `WKP_PK`
	- `WKP_ClosedDate`
	- `WKP_GS_NKProjectManager`
	- `WKP_Module`
	- `WKP_OA_ClientAddress`
	- `WKP_OC_Contact`
	- `WKP_OC_TechnicalContact`
	- `WKP_P8_Opportunity`
	- `WKP_Priority`
	- `WKP_ProjectNumber`
	- `WKP_Status`
	- `WKP_SubType`
	- `WKP_Summary`
	- `WKP_SystemCreateTimeUtc`
	- `WKP_SystemCreateUser`
	- `WKP_SystemLastEditUser`
	- `WKP_Type`
	- `WKP_SystemLastEditTimeUtc`
